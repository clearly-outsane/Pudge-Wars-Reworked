using System;
using Mirror;
using UnityEngine;
using NetworkTransform = Mirror.Experimental.NetworkTransform;

namespace UnityTemplateProjects
{
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(NetworkTransform))]
    public class MovementController : NetworkBehaviour
    {
        public CharacterController characterController;

        public GameObject hookObject;
        [SyncVar]
        public bool isBeingHooked;

        public GameObject otherHook;
        private Vector3 offset;
        

        void OnValidate()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            characterController.enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<NetworkTransform>().clientAuthority = true;
        }

        public override void OnStartLocalPlayer()
        {
            // Camera.main.orthographic = false;
            // Camera.main.transform.SetParent(transform);
            // Camera.main.transform.localPosition = new Vector3(0f, 3f, -8f);
            // Camera.main.transform.localEulerAngles = new Vector3(10f, 0f, 0f);
            offset = Camera.main.transform.position - transform.position;
            characterController.enabled = true;
        }

        void OnDisable()
        {
            if (isLocalPlayer && Camera.main != null)
            {
                // Camera.main.orthographic = true;
                // Camera.main.transform.SetParent(null);
                // Camera.main.transform.localPosition = new Vector3(0f, 70f, 0f);
                // Camera.main.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
            }
        }

        [Header("Movement Settings")]
        public float moveSpeed = 8f;
        public float turnSensitivity = 5f;
        public float maxTurnSpeed = 150f;
        [SerializeField] private float turnSmoothVelocity;
        [SerializeField] private float turnSmoothTime;
        [SerializeField] private float cameraSmoothSpeed=0.5f;

        [Header("Diagnostics")]
        public float horizontal;
        public float vertical;
        public float turn;
        public float jumpSpeed;
        public bool isGrounded = true;
        public bool isFalling;
        public Vector3 velocity;
        public bool isHooking;
        
        [SyncVar]
        public float pullSpeed;
        [SyncVar] 
        public GameObject casterGameObj;
        
        [Command(requiresAuthority = false)]
        public void TriggerCollision(GameObject casterGameObj, float speed)
        {
            isBeingHooked = true;
            
            this.casterGameObj = casterGameObj;
            pullSpeed = speed;
        }
        
        void Update()
        {
            if (!isLocalPlayer || characterController == null || !characterController.enabled)
                return;

            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

           
            
            // Q and E cancel each other out, reducing the turn to zero
            if (Input.GetKey(KeyCode.Q))
                turn = Mathf.MoveTowards(turn, -maxTurnSpeed, turnSensitivity);
            if (Input.GetKey(KeyCode.E))
                turn = Mathf.MoveTowards(turn, maxTurnSpeed, turnSensitivity);
            if (Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.E))
                turn = Mathf.MoveTowards(turn, 0, turnSensitivity);
            if (!Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.E))
                turn = Mathf.MoveTowards(turn, 0, turnSensitivity);
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                isHooking = true;
                Hook();
            }
            
            if (isGrounded)
                isFalling = false;

            if ((isGrounded || !isFalling) && jumpSpeed < 1f && Input.GetKey(KeyCode.Space))
            {
                jumpSpeed = Mathf.Lerp(jumpSpeed, 1f, 0.5f);
            }
            else if (!isGrounded)
            {
                isFalling = true;
                jumpSpeed = 0;
            }
        }

        void Hook()
        {
            CmdSpawnHook(gameObject);
        }

        [Command]
        void CmdSpawnHook(GameObject go, NetworkConnectionToClient conn = null)
        {
            var a = Instantiate(hookObject, go.transform.position + go.transform.forward, go.transform.rotation);
            a.GetComponent<Hook>().casterGameObj = go;
            
            NetworkServer.Spawn(a, conn);
        }
        
        void FixedUpdate()
        {
            if (!isLocalPlayer || characterController == null || !characterController.enabled)
                return;
            
            if (isBeingHooked)
            {
                var dist = Vector3.Distance(casterGameObj.transform.position, transform.position);
                
                if (dist > 2f)
                {
                    transform.LookAt(casterGameObj.transform);
                    transform.Translate(Vector3.forward * 20.0f * Time.deltaTime);
                }
                else
                {
                    isBeingHooked = false;
                    // called only once: kill score update, change my position
                }
            }
            else if (isHooking)
            {
                //player shouldnt move while hooking?
            }
            else
            {
                
                Vector3 direction = new Vector3(horizontal, jumpSpeed, vertical).normalized;


                if (direction.magnitude >= 0.1f)
                {
                    float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                    float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                    transform.rotation = Quaternion.Euler(0f, angle, 0f);
                    
                    Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                    
                    if (jumpSpeed > 0)
                        characterController.Move(direction * Time.fixedDeltaTime);
                    else
                    {
                        characterController.Move(moveDir.normalized*moveSpeed*Time.fixedDeltaTime);
                    }
                }
               

                

                isGrounded = characterController.isGrounded;
                velocity = characterController.velocity;
            }

        }
        
        void LateUpdate()
        {
            Vector3 desiredPosition = transform.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(Camera.main.transform.position, desiredPosition, cameraSmoothSpeed);
            Camera.main.transform.position = smoothedPosition;
       
        }
    }

}