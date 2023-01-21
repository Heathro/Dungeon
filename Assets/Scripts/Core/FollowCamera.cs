using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cinemachine;
using UI;
using Audio;

namespace Core
{
    public class FollowCamera : MonoBehaviour
    {
        // CONFIG

        [SerializeField] AudioListener audioListener;
        [SerializeField] float heightOffset = 1.65f;

        [SerializeField] float normalViewAngle = 35f;
        [SerializeField] float tacticalViewAngle = 90f;
        [SerializeField] float switchTime = 0.01f;

        [SerializeField] float startingDistance = 10f;
        [SerializeField] float minDistance = 5f;
        [SerializeField] float maxDistance = 30f;
        [SerializeField] float scrollSensitivity = 1f;
        [SerializeField] float startingRotateSpeed = 300f;

        [SerializeField] Transform cameraController;
        [SerializeField] float cameraSpeed = 15f;
        [SerializeField] Button resetButton;

        [SerializeField] float transparentingRadius = 5f;

        [SerializeField] GameObject splitMenu;

        // CACHE

        CinemachineVirtualCamera virtualCamera;
        CinemachineFramingTransposer framingTransposer;
        CinemachinePOV pov;
        Transform lastTarget = null;
        Coroutine runningSwitch = null;
        PauseHub pauseMenu = null;
        ShowHideUI showHideUI = null;
        AudioHub audioHub = null;

        // STATE

        public event Action OnDragStart;

        bool negateScrollDirection = false;
        float maxLookaheadTime;
        float lookaheadAdjusted;
        float rotateSpeed;
        bool isControlled = false;
        bool isDragging = false;
        bool isTacticalViewOn = false;

        private int _id;

        // LIFECYCLE

        void Awake()
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
            framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            pov = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
            pov.m_VerticalAxis.m_MinValue = normalViewAngle;
            pov.m_VerticalAxis.m_MaxValue = normalViewAngle;

            EnableResetButton(false);
        }

        void Start()
        {
            pauseMenu = FindObjectOfType<PauseHub>();
            showHideUI = FindObjectOfType<ShowHideUI>();
            audioHub = FindObjectOfType<AudioHub>();

            framingTransposer.m_CameraDistance = startingDistance;
            maxLookaheadTime = framingTransposer.m_LookaheadTime;
            rotateSpeed = startingRotateSpeed;
            lastTarget = virtualCamera.LookAt;
        }

        void LateUpdate()
        {
            VanishObstacles();

            if (pauseMenu.IsPaused()) return;

            CameraTarget();
            AudioListenerPosition();
            CameraRotation();
            CameraDistance();

            if (showHideUI.IsSinglePlayerActionRunning()) return;

            if (Input.GetKeyDown(KeyCode.V))
            {
                SwitchView();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetCameraTargetButton();
            }

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
            {
                if (isControlled) return;

                isControlled = true;
                EnableResetButton(true);
                virtualCamera.LookAt = cameraController;
                virtualCamera.Follow = cameraController;
            }
        }

        // PUBLIC

        public void SwitchView()
        {
            audioHub.PlayClick();

            isTacticalViewOn = !isTacticalViewOn;

            if (runningSwitch != null)
            {
                StopCoroutine(runningSwitch);
            }

            float target = isTacticalViewOn ? tacticalViewAngle : normalViewAngle;
            runningSwitch = StartCoroutine(SwitchViewRoutine(target));
        }

        public void SetTarget(Transform target)
        {
            lastTarget = target;
            isControlled = false;
            virtualCamera.Follow = target;
            virtualCamera.LookAt = target;
        }

        public void ResetCameraTargetButton()
        {
            audioHub.PlayClick();
            ResetCameraTarget();
        }    

        public void ResetCameraTarget()
        {
            isControlled = false;
            EnableResetButton(false);
            SetTarget(lastTarget);
        }

        public bool IsDragging()
        {
            return isDragging;
        }

        public void SetDragging(bool isDragging)
        {
            if (isDragging && OnDragStart != null)
            {
                OnDragStart();
            }

            this.isDragging = isDragging;
        }

        public float GetZoomPercentage()
        {
            return (framingTransposer.m_CameraDistance - 5f) * 4f;
        }

        // PRIVATE

        void CameraTarget()
        {
            cameraController.rotation = audioListener.transform.rotation;

            if (!isControlled)
            {
                cameraController.position = virtualCamera.LookAt.position;
                return;
            }

            Vector3 vect = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            cameraController.Translate(Vector3.ClampMagnitude(vect, 1f) * cameraSpeed * Time.deltaTime);
        }

        void CameraRotation()
        {
            if (!isDragging && Input.GetMouseButton(1) && !splitMenu.activeSelf)
            {
                pov.m_HorizontalAxis.m_MaxSpeed = rotateSpeed;
            }
            else
            {
                pov.m_HorizontalAxis.m_MaxSpeed = 0f;
            }
        }

        void CameraDistance()
        {
            if (!EventSystem.current.IsPointerOverGameObject() && Input.mouseScrollDelta.y != 0)
            {
                var scroll = Input.mouseScrollDelta.y;
                var adjustedScroll = scroll * scrollSensitivity;
                NewCameraDistance(adjustedScroll);
            }
        }        

        void NewCameraDistance(float adjustedScroll)
        {
            if (negateScrollDirection)
            {
                framingTransposer.m_CameraDistance =
                    Mathf.Clamp(framingTransposer.m_CameraDistance += adjustedScroll, minDistance, maxDistance);
            }
            else
            {
                framingTransposer.m_CameraDistance =
                    Mathf.Clamp(framingTransposer.m_CameraDistance -= adjustedScroll, minDistance, maxDistance);
            }
            
            if (maxLookaheadTime == 0) return;
            lookaheadAdjusted = Mathf.Clamp(framingTransposer.m_CameraDistance / maxDistance, 0.1f, maxLookaheadTime);
            framingTransposer.m_LookaheadTime = lookaheadAdjusted;
        }

        void AudioListenerPosition()
        {
            audioListener.transform.position = virtualCamera.LookAt.position + Vector3.up * heightOffset;

            float y = Camera.main.transform.rotation.y;
            float w = Camera.main.transform.rotation.w;
            audioListener.transform.rotation = new Quaternion(0, y, 0, w);
        }

        IEnumerator SwitchViewRoutine(float target)
        {
            while (!Mathf.Approximately(pov.m_VerticalAxis.m_MinValue, target) && !Mathf.Approximately(pov.m_VerticalAxis.m_MaxValue, target))
            {
                pov.m_VerticalAxis.m_MinValue = Mathf.MoveTowards(pov.m_VerticalAxis.m_MinValue, target, Time.deltaTime / switchTime);
                pov.m_VerticalAxis.m_MaxValue = Mathf.MoveTowards(pov.m_VerticalAxis.m_MaxValue, target, Time.deltaTime / switchTime);
                yield return null;
            }
        }

        void EnableResetButton(bool isEnable)
        {
            resetButton.gameObject.SetActive(isEnable);
        }

        void VanishObstacles()
        {
            Vector3 camera = Camera.main.transform.position;
            Vector3 player = cameraController.position + heightOffset * Vector3.up;
            Vector3 direction = -(camera - player).normalized;
            float vanishingDistance = Vector3.Distance(camera, player) - transparentingRadius - 1f;

            RaycastHit[] hits = Physics.SphereCastAll(camera, transparentingRadius, direction, vanishingDistance);

            foreach (RaycastHit hit in hits)
            {
                VanishingObject vanishingObject = hit.transform.GetComponent<VanishingObject>();

                if (vanishingObject != null) vanishingObject.Vanish();
            }
        }
    }
}