using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class KartController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Transform[] _rayPoints;
    [SerializeField] private LayerMask _drivable;
    [SerializeField] private Transform _accelerationPoint;
    [SerializeField] private GameObject[] _tires = new GameObject[4];
    [SerializeField] private GameObject[] _frontTireParents = new GameObject[2];




    [Header("Configuracion de suspencion")]
    [SerializeField] private float _restLength;
    [SerializeField] private float _springTravel;
    [SerializeField] private float _springStiffess;
    [SerializeField] private float _damperStiffness;
    [SerializeField] private float _wheelRadius;

    private int[] _wheelIsGrounded = new int[4];
    private bool _isGrounded = false;

    [Header("Input")]
    private float _moveInput = 0;
    private float _steerInput = 0;

    [Header("Configuracion Auto")]
    [SerializeField] private float _acceleration = 25f;
    [SerializeField] private float _maxSpeed = 100f;
    [SerializeField] private float _deceleration = 10f;
    [SerializeField] private float _steerStrength = 15f;
    [SerializeField] private AnimationCurve _turningCurve;
    [SerializeField] private float _dragCoefficient = 1f;

    private Vector3 _currentCarLocalVelocity = Vector3.zero;
    private float _carVelocityRatio = 0;

    [Header("Visuales")]
    [SerializeField] private float _tireRotSpeed = 3000f;
    [SerializeField] private float _maxSteeringAngle = 30f;



    #region Funciones
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        Suspension();
        GroundCheck();
        CalculatecarVelocity();
        Movement();
        Visuals();
    }
    void Update()
    {
        GetPlayerInput();
    }
    #endregion

    #region Movimiento
    private void Movement()
    {
        if (_isGrounded)
        {
            Acceleration();
            Deceleration();
            Turn();
            SidewaysDarg();
        }
    }
    private void Acceleration()
    {
        _rb.AddForceAtPosition(_acceleration * _moveInput * transform.forward, _accelerationPoint.position, ForceMode.Acceleration);
    }
    private void Deceleration()
    {
        _rb.AddForceAtPosition(_deceleration * _moveInput * -transform.forward, _accelerationPoint.position, ForceMode.Acceleration);
    }
    private void Turn()
    {
        _rb.AddTorque(_steerStrength * _steerInput * _turningCurve.Evaluate(_carVelocityRatio) * Mathf.Sign(_carVelocityRatio) * transform.up, ForceMode.Acceleration);
    }
    private void SidewaysDarg()
    {
        float currentSidewaysSpeed = _currentCarLocalVelocity.x;
        float dragMagnitude = -currentSidewaysSpeed * _dragCoefficient;
        Vector3 dragForce = transform.right * dragMagnitude;
        _rb.AddForceAtPosition(dragForce, _rb.worldCenterOfMass, ForceMode.Acceleration);
    }
    #endregion

    #region Visuales
    private void Visuals()
    {
        TireVisuals();
    }
    private void TireVisuals()
    {
        float steeringAngle = _maxSteeringAngle * _steerInput;

        for (var i = 0; i < _tires.Length; i++)
        {
            if (i < 2)
            {
                _tires[i].transform.Rotate(Vector3.right, _tireRotSpeed * _carVelocityRatio * Time.deltaTime, Space.Self);
                _frontTireParents[i].transform.localEulerAngles = new Vector3(_frontTireParents[i].transform.localEulerAngles.x, steeringAngle, _frontTireParents[i].transform.localEulerAngles.z);
            }
            else
            {
                _tires[i].transform.Rotate(Vector3.right, _tireRotSpeed * _moveInput * Time.deltaTime, Space.Self);
            }
        }
    }
    private void SetTirePosition(GameObject tire, Vector3 targetPosition)
    {
        tire.transform.position = targetPosition;
    }
    #endregion

    #region Estado del auto
    private void GroundCheck()
    {
        int tempGroundedWheels = 0;
        for (int i = 0; i < _wheelIsGrounded.Length; i++)
        {
            tempGroundedWheels += _wheelIsGrounded[i];
        }
        if (tempGroundedWheels > 1)
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }
    }
    private void CalculatecarVelocity()
    {
        _currentCarLocalVelocity = transform.InverseTransformDirection(_rb.linearVelocity);
        _carVelocityRatio = _currentCarLocalVelocity.z / _maxSpeed;
    }
    #endregion

    #region Input Handlign
    private void GetPlayerInput()
    {
        _moveInput = Input.GetAxis("Vertical");
        _steerInput = Input.GetAxis("Horizontal");
    }
    #endregion

    #region Suspension
    private void Suspension()
    {
        for (int i = 0; i < _rayPoints.Length; i++)
        {
            RaycastHit hit;
            float maxDistance = _restLength + _springTravel;

            if (Physics.Raycast(_rayPoints[i].position, -_rayPoints[i].up, out hit, maxDistance + _wheelRadius, _drivable))
            {
                _wheelIsGrounded[i] = 1;

                float currentSpringLength = hit.distance - _wheelRadius;
                float springCompression = (_restLength - currentSpringLength) / _springTravel;

                float springVelocity = Vector3.Dot(_rb.GetPointVelocity(_rayPoints[i].position), _rayPoints[i].up);
                float dampForce = _damperStiffness * springVelocity;

                float springForce = _springStiffess * springCompression;
                float netForce = springForce - dampForce;

                _rb.AddForceAtPosition(netForce * _rayPoints[i].up, _rayPoints[i].position);

                //Visuales
                SetTirePosition(_tires[i], hit.point + _rayPoints[i].up * _wheelRadius);

                Debug.DrawLine(_rayPoints[i].position, hit.point, Color.red);
            }
            else
            {
                _wheelIsGrounded[i] = 0;

                //Visuales
                SetTirePosition(_tires[i], _rayPoints[i].position - _rayPoints[i].up * maxDistance);

                Debug.DrawLine(_rayPoints[i].position, _rayPoints[i].position + (_wheelRadius + maxDistance) * -_rayPoints[i].up, Color.green);
            }
        }
    }
    #endregion
}
