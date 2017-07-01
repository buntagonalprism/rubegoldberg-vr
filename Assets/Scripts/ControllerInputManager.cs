using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ControllerInputManager : MonoBehaviour {

    [Header("SteamVR Camera Rig Components")]
    public ControllerInputDetector leftHand;
    public ControllerInputDetector rightHand;
    public GameObject playerHead;

    [Header("Device Menu Components")]
    public GameObject deviceMenu;
    private List<GameObject> deviceMenuItems = new List<GameObject>();
    public List<GameObject> devicePrefabs;
    public List<int> remainingDeviceCount;


    [Header("Teleport and UI Pointing")]
    public GameObject teleportTarget;
    public LayerMask laserMask;
    public LayerMask uiMask;
    public int maxTeleportRange = 15;

    [Header("Teleport, Movement and Throwing Speeds")]
    public float dashVelocity = 6f;
    public float walkVelocity = 1f;
    public float throwBoost = 1.4f;

    [Header("UI Screens")]
    public Canvas[] welcomeScreens;
    public Canvas winScreen;
    public Canvas looseScreen;
    public string nextLevelName;

    // Represents containing player object (i.e. the SteamVR camera rig)
    private GameObject player;

    // Game management
    private BallController ball;
    private CollectibleManager collectibleManager;
    private Goal goal;
    private int totalDeviceCount;

    // Teleport management
    private LineRenderer teleportBeamLine;
    private int maxRangeVertical;
    private Vector3 teleportLocation;
    private float targetHeightAdjust;

    // Teleport dash tracking
    private bool isMoving;
    private Vector3 moveDirection;
    private float totalTravelTime;
    private float elapsedTravelTime = 0f;

    
    // UI Menu tracking
    private int uiIdx = 0;
    private bool uiMenuShowing = true;
    private bool uiMenuHit = false;
    private Button uiMenuHighlighted = null;
    private bool hasClickedUi = false;  // Used to stop teleporting after clicking a UI button

    // Device menu tracking
    private bool deviceMenuShowing = false;
    private int currentIdx = 0;
    private GameObject placingObj = null;
    private List<GameObject> holdingObjs = new List<GameObject>();

    // Use this for initialization
    void Start()
    {
        collectibleManager = FindObjectOfType<CollectibleManager>();
        ball = FindObjectOfType<BallController>();
        goal = FindObjectOfType<Goal>();
        if (goal != null) goal.GoalEntered += GoalEntered;

        targetHeightAdjust = 0.05f;
        maxRangeVertical = (int)Mathf.Round(maxTeleportRange * 1.2f);
        teleportBeamLine = GetComponentInChildren<LineRenderer>();
        player = gameObject;
        leftHand.OnGripPress += WalkForward;
        leftHand.OnTouchpad += ShowTeleportBeamLeft;
        leftHand.OnTouchpadUp += StartDashing;
        leftHand.OnTouchpadPressDown += ClickUiButton;
        leftHand.OnColliderTriggerStay += new ControllerInputDetector.TriggerHandler(GrabObjectLeft);
        leftHand.OnTriggerPressUp += ReleaseObjectsLeft;
        rightHand.OnColliderTriggerStay += new ControllerInputDetector.TriggerHandler(GrabObjectRight);
        rightHand.OnTouchpadDown += ShowDeviceMenu;
        rightHand.OnTouchpadUp += HideDeviceMenu;
        rightHand.OnTouchpadPressDown += CycleOrCreateDevice;
        rightHand.OnTouchpadPressUp += PlaceDevice;
        rightHand.OnTriggerPressUp += ReleaseObjectsRight;
        //rightHand.OnTriggerPressDown += MenuRightTrigger;

        int idx = 0;
        foreach (Transform child in deviceMenu.transform)
        {
            Canvas canvas = child.gameObject.GetComponentInChildren<Canvas>();
            deviceMenuItems.Add(canvas.gameObject);
            Text text = canvas.GetComponentInChildren<Text>();
            int count = remainingDeviceCount[idx++];
            text.text = text.text + " (" + count + ")";
            if (count == 0)
                text.color = new Color(1f, 0f, 0f);
        }
        for (int i = 1; i < deviceMenuItems.Count; i++)
        {
            deviceMenuItems[i].SetActive(false);
        }
        deviceMenu.SetActive(false);
        remainingDeviceCount.ForEach(delegate (int i) { totalDeviceCount += i; });
    }



    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            DashForward();
            return;
        }
    }
    void GoalEntered()
    {
        if (collectibleManager.collectiblesRemaining == 0)
        {
            WinLevel();
        }
        else
        {
            LooseLevel();
        }
    }

    void WinLevel()
    {
        AudioSource.PlayClipAtPoint(goal.successClip, transform.position);
        winScreen.gameObject.SetActive(true);
        Text[] texts = winScreen.gameObject.GetComponentsInChildren<Text>();
        foreach (Text text in texts)
        {
            if (text.gameObject.name == "Time Taken")
            {
                float totalTime = Time.timeSinceLevelLoad;
                int minutes = (int) Mathf.Floor(totalTime / 60f);
                int seconds = ((int) totalTime) % 60;
                text.text = minutes + "m " + seconds + "s";
            }
            else if (text.gameObject.name == "Devices Used")
            {
                int devicesRemaining = 0;
                remainingDeviceCount.ForEach(delegate (int i) { devicesRemaining += i; });
                int devicesUsed = totalDeviceCount - devicesRemaining;
                text.text = devicesUsed.ToString() + " / " + totalDeviceCount.ToString();
            }
            else if (text.gameObject.name == "Ball Resets")
            {
                text.text = ball.totalResets.ToString();
            }
        }
    }

    void LooseLevel()
    {
        AudioSource.PlayClipAtPoint(goal.failureClip, transform.position);
        looseScreen.gameObject.SetActive(true);
        Text[] texts = looseScreen.gameObject.GetComponentsInChildren<Text>();
        foreach (Text text in texts)
        {
            if (text.gameObject.name == "Stars Collected")
            {
                int remaining = collectibleManager.collectiblesRemaining;
                int total = collectibleManager.totalCollectibles;
                int collected = total - remaining;
                text.text = collected.ToString() + " / " + total.ToString();
            }
        }
    }


    void ShowDeviceMenu(float x, float y)
    {
        deviceMenu.SetActive(true);
        deviceMenuShowing = true;
    }
    void HideDeviceMenu(float x, float y)
    {
        deviceMenu.SetActive(false);
        deviceMenuShowing = false;
    }
    void CycleOrCreateDevice(float x, float y)
    {
        // Press left - Cycle left
        if (x < -0.5f && y < 0.5f && y > -0.5f)
        {
            deviceMenuItems[currentIdx].SetActive(false);
            currentIdx--;
            if (currentIdx < 0)
                currentIdx = deviceMenuItems.Count - 1;
            deviceMenuItems[currentIdx].SetActive(true);
        }

        // Press Right - Cycle right
        else if (x > 0.5f && y < 0.5f && y > -0.5f)
        {
            deviceMenuItems[currentIdx].SetActive(false);
            currentIdx++;
            if (currentIdx >= deviceMenuItems.Count)
                currentIdx = 0;
            deviceMenuItems[currentIdx].SetActive(true);
        }
        // Press Up - create object
        else if (y > 0.5f && x < 0.5f && x > -0.5f)
        {
            // Check if we have placed too many objects for this type
            if (remainingDeviceCount[currentIdx] != 0)
            {
                HideDeviceMenu(-1f, -1f);
                placingObj = Instantiate(devicePrefabs[currentIdx], rightHand.transform);
                placingObj.transform.SetParent(rightHand.transform, true);
                // Update text
                remainingDeviceCount[currentIdx]--;
                Text text = deviceMenuItems[currentIdx].GetComponentInChildren<Text>();
                string pattern = "\\(\\d+\\)";
                Regex rgx = new Regex(pattern);
                text.text = rgx.Replace(text.text, "(" + remainingDeviceCount[currentIdx] + ")");

                // Set red error text
                if (remainingDeviceCount[currentIdx] == 0)
                    deviceMenuItems[currentIdx].GetComponentInChildren<Text>().color = new Color(1f, 0f, 0f);
            }
        }
    }

    // Release the object we are placing
    void PlaceDevice(float x, float y)
    {
        if (placingObj != null)
        {
            placingObj.transform.SetParent(null);
            placingObj = null;
        }
    }

    void ShowTeleportBeamLeft(float x, float y)
    {
        ShowTeleportBeam(leftHand);
    }

    void ShowTeleportBeam(ControllerInputDetector hand)
    {
        
        if (hasClickedUi) {
            teleportTarget.gameObject.SetActive(false);
            teleportBeamLine.gameObject.SetActive(false);
            return;
        }
        teleportBeamLine.gameObject.SetActive(true);

        if (uiMenuShowing) {
            uiMenuHit = ShowMenuPointer(hand);
        } else
        {
            uiMenuHit = false;
        }
        if (!uiMenuHit)
        {
            ShowTeleportPointer(hand);
        }
    }

    bool ShowMenuPointer(ControllerInputDetector hand)
    {
        RaycastHit hit;
        if (Physics.Raycast(hand.transform.position, hand.transform.forward, out hit, maxTeleportRange, uiMask))
        {
            // Selectable the button to hilight it - or we set colour manually on the image instead of the button
            //hit.transform.GetComponent<Button>().Select();
            teleportTarget.gameObject.SetActive(true);
            teleportBeamLine.startColor = new Color(0f, 0f, 1f);
            teleportBeamLine.endColor = new Color(0f, 0f, 1f);
            teleportBeamLine.SetPosition(0, hand.transform.position);
            teleportBeamLine.SetPosition(1, hit.point);

            teleportTarget.gameObject.SetActive(false);
            ApplyUiHighlight(hit);
            return true;
        }
        RemoveUiHighlight();
        uiMenuHit = false;
        return false;
    }

    public void AdvanceUiScreen()
    {
        RemoveUiHighlight();
        welcomeScreens[uiIdx++].gameObject.SetActive(false);
        if (uiIdx < welcomeScreens.Length)
        {
            welcomeScreens[uiIdx].gameObject.SetActive(true);
        }
    }

    public void BackUiScreen()
    {
        RemoveUiHighlight();
        welcomeScreens[uiIdx--].gameObject.SetActive(false);
        if (uiIdx >= 0)
        {
            welcomeScreens[uiIdx].gameObject.SetActive(true);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    
    public void AdvanceLevel()
    {
        HideScreens();
        //string nextSceneName = SceneManager.GetSceneAt(SceneManager.GetActiveScene().buildIndex + 1).name;
        SteamVR_LoadLevel.Begin(nextLevelName);
    }


    public void RetryLevel()
    {
        SteamVR_LoadLevel.Begin(SceneManager.GetActiveScene().name);
    }

    public void HideScreens()
    {
        if (winScreen != null) winScreen.gameObject.SetActive(false);
        if (looseScreen != null) looseScreen.gameObject.SetActive(false);
        foreach (Canvas canvas in welcomeScreens) canvas.gameObject.SetActive(false);
        // Remove pointer beam so we don't immediately jump
        hasClickedUi = true;
        teleportTarget.gameObject.SetActive(false);
        teleportBeamLine.gameObject.SetActive(false);
    }

    void ClickUiButton(float x, float y)
    {
        if (uiMenuHighlighted != null)
        {
            // Invoke the button click event
            uiMenuHighlighted.onClick.Invoke();
        }
    }

    void ShowTeleportPointer(ControllerInputDetector hand)
    {
        RaycastHit hit;
        teleportTarget.gameObject.SetActive(true);

        // See if our controller ray hits something
        if (Physics.Raycast(hand.transform.position, hand.transform.forward, out hit, maxTeleportRange, laserMask))
        {
            teleportLocation = hit.point;
        }
        // If it doesn't, take the point at maximum range and project straight down, see if that hits something
        else
        {
            Vector3 maxPosition = hand.transform.position + (maxTeleportRange * hand.transform.forward);
            RaycastHit maxHit;
            if (Physics.Raycast(maxPosition, Vector3.down, out maxHit, maxRangeVertical, laserMask))
            {
                teleportLocation = maxHit.point;
            }
            // If nothing valid hit here, no else condition - instead we'll use the transform from last frame. 
        }

        // Draw the line to the teleport target, and move the teleport target. 
        teleportBeamLine.startColor = new Color(1f, 0f, 0f);
        teleportBeamLine.endColor = new Color(1f, 0f, 0f);
        teleportBeamLine.SetPosition(0, hand.transform.position);
        teleportBeamLine.SetPosition(1, teleportLocation);
        teleportTarget.transform.position = teleportLocation + new Vector3(0, targetHeightAdjust, 0);
    }

    void StartDashing(float x, float y)
    {
        // Releasing the touchpad after clicking a UI button does nothing
        if (hasClickedUi)
        {
            hasClickedUi = false;
            return;
        }
        teleportBeamLine.gameObject.SetActive(false);
        teleportTarget.gameObject.SetActive(false);
        if (!uiMenuHit)
        {
            Vector3 teleportVector = teleportLocation - player.transform.position;
            moveDirection = teleportVector / teleportVector.magnitude;
            totalTravelTime = teleportVector.magnitude / dashVelocity;
            isMoving = true;
        }
        else
        {
            RemoveUiHighlight();
        }
    }

    void ApplyUiHighlight(RaycastHit hit)
    {
        if (hit.transform.GetComponent<Button>())
        {
            uiMenuHighlighted = hit.transform.GetComponent<Button>();
            uiMenuHighlighted.GetComponent<Image>().color = new Color(0f, 0.8f, 1f);
        } else
        {
            RemoveUiHighlight();
        }
    }

    void RemoveUiHighlight()
    {
        if (uiMenuHighlighted != null)
        {
            uiMenuHighlighted.GetComponent<Image>().color = new Color(1f, 1f, 1f);
            uiMenuHighlighted = null;
        }
    }

    void DashForward()
    {
        player.transform.position += Time.deltaTime * moveDirection * dashVelocity;
        elapsedTravelTime += Time.deltaTime;
        if (elapsedTravelTime >= totalTravelTime)
        {
            isMoving = false;
            elapsedTravelTime = 0;
        }
    }

    void WalkForward()
    {
        // Project the viewing direction forward according to the walk velocity and frame time
        Vector3 viewPosition = playerHead.transform.position + (Time.deltaTime * walkVelocity * playerHead.transform.forward);
        RaycastHit currentGroundHit;
        if (Physics.Raycast(playerHead.transform.position, Vector3.down, out currentGroundHit, maxTeleportRange, laserMask))
        {
            RaycastHit walkHit;
            // See if there is valid ground in front to walk onto
            if (Physics.Raycast(viewPosition, Vector3.down, out walkHit, maxTeleportRange, laserMask))
            {
                // Adjust the camera rig by the difference
                player.transform.position += walkHit.point - currentGroundHit.point;
            }
        }
    }

    void GrabObjectLeft(Collider collider)
    {
        GrabObject(collider, leftHand);
    }

    // Trigger pulls inside colliders should be ignored when our object menu is showing
    void GrabObjectRight(Collider collider)
    {
        if (!deviceMenuShowing)
            GrabObject(collider, rightHand);
    }

    void GrabObject(Collider collider, ControllerInputDetector hand)
    {
        if (collider.gameObject.CompareTag("Throwable") || collider.gameObject.CompareTag("Placeable") || collider.gameObject.CompareTag("PlaceableChild"))
        {
            // Grab hold of throwable or placeable objects when trigger held
            if (hand.IsTriggerDown())
            {
                bool keepWorldPosition = true;
                // Used when sub-colliders are used to build parent object shape. 
                if (collider.gameObject.CompareTag("PlaceableChild"))
                {
                    collider.transform.parent.transform.SetParent(hand.transform, keepWorldPosition);
                    holdingObjs.Add(collider.transform.parent.gameObject);
                    // Disable child colliders to prevent child trigger events being passed up to the controller
                    Collider[] childColliders = collider.transform.parent.GetComponentsInChildren<Collider>();
                    foreach (Collider childCollider in childColliders)
                    {
                        childCollider.enabled = false;
                    }
                }
                // Throwable and placeable objects
                else
                {
                    collider.transform.SetParent(hand.transform, keepWorldPosition);
                    collider.GetComponent<Rigidbody>().isKinematic = true;
                    holdingObjs.Add(collider.gameObject);
                }
                hand.HapticPulse(2000);

            }
        }
    }

    void ReleaseObjectsRight()
    {
        ReleaseObjects(rightHand);
    }

    void ReleaseObjectsLeft()
    {
        ReleaseObjects(leftHand);
    }

    void ReleaseObjects(ControllerInputDetector hand)
    {
        foreach (GameObject heldObj in holdingObjs) { 
            // Throw by imparting controller velocity to held object
            if (heldObj.CompareTag("Throwable"))
            {
                heldObj.transform.SetParent(null);
                Rigidbody rigidBody = heldObj.GetComponent<Rigidbody>();
                rigidBody.isKinematic = false;
                rigidBody.velocity = hand.GetVelocity() * throwBoost;
                rigidBody.angularVelocity = hand.GetAngularVelocity();
            }
            // Place by removing parent transform relationship
            else
            {
                heldObj.transform.SetParent(null);
                Collider[] childColliders = heldObj.GetComponentsInChildren<Collider>();
                foreach (Collider childCollider in childColliders)
                {
                    childCollider.enabled = true;
                }
            }
        }
    }

}
