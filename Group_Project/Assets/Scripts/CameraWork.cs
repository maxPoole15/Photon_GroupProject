using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;


namespace Com.MyCompany.MyGame
{
    /// <summary>
    /// Camera work. Follow a target
    /// </summary>
    public class CameraWork : MonoBehaviour
    {
        #region Private Fields


        [Tooltip("The distance in the local x-z plane to the target")]
        [SerializeField]
        private float distance = 0.2f;


        [Tooltip("The height we want the camera to be above the target")]
        [SerializeField]
        private float height = 2.0f;


        [Tooltip("Allow the camera to be offseted vertically from the target, for example giving more view of the sceneray and less ground.")]
        [SerializeField]
        private Vector3 centerOffset = Vector3.zero;


        [Tooltip("Set this as false if a component of a prefab being instanciated by Photon Network, and manually call OnStartFollowing() when and if needed.")]
        [SerializeField]
        private bool followOnStart = false;


        [Tooltip("The Smoothing for the camera to follow the target")]
        [SerializeField]
        private float smoothSpeed = 0.125f;


        // cached transform of the target
        Transform cameraTransform;


        // maintain a flag internally to reconnect if target is lost or camera is switched
        bool isFollowing;


        // Cache for camera offset
        Vector3 cameraOffset = Vector3.zero;


        #endregion


		public Camera[] cameras;
        //private Camera cam;
	
		public List<GameObject> players;
		
		public PhotonView pv;

        #region MonoBehaviour Callbacks


        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase
        /// </summary>
        void Start()
        {
            // Start following the target if wanted.
            if (followOnStart)
            {
                OnStartFollowing();
            }
            
        }


        void LateUpdate()
        {
			
            // The transform target may not destroy on level load,
            // so we need to cover corner cases where the Main Camera is different everytime we load a new scene, and reconnect when that happens
            if (cameraTransform == null && isFollowing)
            {
                OnStartFollowing();
            }


            // only follow is explicitly declared
            if (isFollowing)
            {
                Follow();
            }
        }


        #endregion


        #region Public Methods


        /// <summary>
        /// Raises the start following event.
        /// Use this when you don't know at the time of editing what to follow, typically instances managed by the photon network.
        /// </summary>
        public void OnStartFollowing()
        {
            cameraTransform = Camera.main.transform;
            isFollowing = true;
            // we don't smooth anything, we go straight to the right camera shot
            Cut();
        }


        #endregion


        #region Private Methods


        /// <summary>
        /// Follow the target smoothly
        /// </summary>
        void Follow()
        {
            cameraOffset.z = -distance;
            cameraOffset.y = height;


            cameraTransform.position = Vector3.Lerp(cameraTransform.position, this.transform.position + this.transform.TransformVector(cameraOffset), smoothSpeed * Time.deltaTime);

            cameraTransform.LookAt(this.transform.position + centerOffset);

            for (int i = 0; i < players.Count; i++)
            {
                cameras[i].transform.parent = players[i].transform;
                //cameras[i].transform.position = players[i].transform.position + players[i].transform.TransformVector(cameraOffset);
                /*if (i == 1)
                {
                    if (players.Count > 1)
                        
                }
                else if (i == 2)
                {
                    if (players.Count > 2)
                        cameras[i].transform.position = players[i - 1].transform.position + players[i - 1].transform.TransformVector(cameraOffset);
                }
                else if (i == 3)
                {
                    if (players.Count > 3)
                        cameras[i].transform.position = players[i - 1].transform.position + players[i - 1].transform.TransformVector(cameraOffset);
                }*/
            }
        }


        void Cut()
        {
            cameras = Camera.allCameras;
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                players.Add(player);
            }
            cameraOffset.z = -distance;
            cameraOffset.y = height;


            cameraTransform.position = this.transform.position + this.transform.TransformVector(cameraOffset);

            cameraTransform.LookAt(this.transform.position + centerOffset);
			
			for(int i = 0; i < cameras.Length; i++)
			{
                if(i == 0)
                {
                    cameras[i].rect = new Rect(0f, .5f, .5f, .5f);
                }
                else if(i == 1)
                {
                    cameras[i].rect = new Rect(.5f, .5f, .5f, .5f);
                    if (players.Count > 1)
                        cameras[i].transform.parent = players[i].transform;
                        //cameras[i].transform.position = players[i].transform.position + players[i].transform.TransformVector(cameraOffset);
                }
                else if (i == 2)
                {
                    cameras[i].rect = new Rect(0f, 0f, .5f, .5f);
                    if (players.Count > 2)
                        cameras[i].transform.parent = players[i].transform;
                        //cameras[i].transform.position = players[i].transform.position + players[i].transform.TransformVector(cameraOffset);
                }
                else
                {
                    cameras[i].rect = new Rect(.5f, 0f, .5f, .5f);
                    if (players.Count > 3)
                        cameras[i].transform.parent = players[i].transform;
                        //cameras[i].transform.position = players[i].transform.position + players[i].transform.TransformVector(cameraOffset);
                }
            }
        }
        #endregion
    }
}