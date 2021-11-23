using UnityEngine;

using Photon.Pun;

using System.Collections;

namespace Com.MyCompany.MyGame
{
    /// <summary>
    /// Player manager.
    /// Handles fire Input and Beams.
    /// </summary>
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region IPunObservable implementation


        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(shooting);
                stream.SendNext(Health);
            }
            else
            {
                // Network player, receive data
                this.shooting = (bool)stream.ReceiveNext();
                this.Health = (float)stream.ReceiveNext();
            }
        }


        #endregion
        #region Public Fields

        [Tooltip("The current Health of our player")]
        public float Health = 1f;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField]
        public GameObject PlayerUiPrefab;

        public GameObject DeadScreen;

        private bool isDead;
        Canvas canvas;
        #endregion


        #region Private Fields

        [Tooltip("The Beams GameObject to control")]
        [SerializeField]
        private GameObject beams;
        #endregion

        #region Private Methids

        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
        {
             this.CalledOnLevelWasLoaded(scene.buildIndex);
        }
        #endregion

        #region MonoBehaviour CallBacks

        void OnLevelWasLoaded(int level)
        {
            this.CalledOnLevelWasLoaded(level);
        }
        void CalledOnLevelWasLoaded(int level)
        {
            
            // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0f, 5f, 0f);
            }
            //GameObject _uiGo = Instantiate(this.PlayerUiPrefab);
            //_uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            canvas = GetComponent<Canvas>();
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        void Awake()
        {
            if (beams == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> Beams Reference.", this);
            }
            else
            {
                beams.SetActive(false);
            }
            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
            if (photonView.IsMine)
            {
                
                PlayerManager.LocalPlayerInstance = this.gameObject;
            }
            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(this.gameObject);
        }
        void Start()
        {
            CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();
            DeadScreen = GameObject.Find("DeadScreen");
            DeadScreen.SetActive(false);

            if (_cameraWork != null)
            {
                if (photonView.IsMine)
                {
                    _cameraWork.OnStartFollowing();
                }
            }
            else
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
            }

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

            /*if (PlayerUiPrefab != null)
            {
                GameObject _uiGo = Instantiate(PlayerUiPrefab);
                _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            }
            else
            {
                Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
            }*/
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// </summary>
        void Update()
        {
            Debug.Log(Health + "   " + isDead);
            if (photonView.IsMine && !isDead)
            {
                
                // trigger Beams active state
                if (beams != null && shooting != beams.activeInHierarchy)
                {
                    beams.SetActive(shooting);
                }
                ProcessInputs();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                Respawn();
            }
        }
        public void Respawn()
        {
            isDead = false;
            Health = 1f;
            DeadScreen = GameObject.Find("DeadScreen");
            DeadScreen.SetActive(false);
            reloading = false;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!photonView.IsMine)
            {
                return;
            }
            
            // We are only interested in Beamers
            // we should be using tags but for the sake of distribution, let's simply check by name.
            if (!other.name.Contains("Beam"))
            {
                return;
            }
            isDead = true;
            Health -= 1f;
            beams.SetActive(false);
            DeadScreen.SetActive(true);
        }
        /// <summary>
        /// MonoBehaviour method called once per frame for every Collider 'other' that is touching the trigger.
        /// We're going to affect health while the beams are touching the player
        /// </summary>
        /// <param name="other">Other.</param>
        /*void OnTriggerStay(Collider other)
        {
            // we dont' do anything if we are not the local player.
            if (!photonView.IsMine)
            {
                return;
            }
            // We are only interested in Beamers
            // we should be using tags but for the sake of distribution, let's simply check by name.
            if (!other.name.Contains("Beam"))
            {
                return;
            }
        }*/

        #endregion

        #region Custom

        /// <summary>
        /// Processes the inputs. Maintain a flag representing when the user is pressing Fire.
        /// </summary>
        /// 
        private bool shooting = false;
        private bool reloading = false;

        private float timeSinceShot = 0f;
        private float timeUntilShot = 5f;

        public LayerMask wallMask;
        public LayerMask playerMask;
        void ProcessInputs()
        {

            // OLD SHOOTING

            /*if (Input.GetButtonDown("Fire1"))
            {
                if (!IsFiring)
                {
                    IsFiring = true;
                }
            }
            if (Input.GetButtonUp("Fire1"))
            {
                if (IsFiring)
                {
                    IsFiring = false;
                }
            }*/

            // NEW SHOOTING

            if (!isDead && photonView.IsMine)
            {
                Debug.DrawRay(transform.position, transform.forward);

                if (reloading)
                {
                    shooting = false;
                    timeSinceShot = timeSinceShot + Time.deltaTime;
                    if (timeSinceShot >= timeUntilShot)
                    {
                        reloading = false;
                        timeSinceShot = 0f;
                    }
                }
                if (Input.GetMouseButtonDown(0) && !reloading)
                {
                    shooting = true;
                }
                if (Input.GetMouseButtonUp(0))
                {
                    shooting = false;
                }

                if (shooting && !reloading)
                {
                    Debug.DrawRay(transform.position, transform.forward);

                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    //Ray ray = new Ray(transform.position, transform.forward);

                    if (Physics.Raycast(ray, out hit, 500, playerMask, QueryTriggerInteraction.Ignore))
                    {
                        //Player was hit by raycast
                        Debug.Log("Might have hit a player");

                        GameObject playerHit = hit.collider.gameObject;
                        Vector3 dist = playerHit.transform.position - transform.position;

                        Ray ray2 = new Ray(transform.position, dist);
                        if (Physics.Raycast(ray, out hit, 500, wallMask, QueryTriggerInteraction.Ignore))
                        {
                            //Hit a wall first
                            Debug.Log("Hit a wall first");
                        }
                        else
                        {
                            //Hit a player first
                            Debug.Log("Hit a Player");

                            //Kill the player here
                        }
                    }

                    //Reload starts whether you hit anything or not
                    reloading = true;
                }
            }
            
        }

        public override void OnDisable()
        {
            // Always call the base to remove callbacks
            base.OnDisable();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        #endregion
    }
}