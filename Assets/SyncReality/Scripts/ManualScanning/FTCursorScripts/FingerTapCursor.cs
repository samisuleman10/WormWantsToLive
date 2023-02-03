using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



public class FingerTapCursor : MonoBehaviour
{

    public class FingerTapEventParameters
    {
        public FingerTapActionType ActionType;
        public FingerTapEvent EventtoActivate;
        public GameObject GameData;
        public GameObject SelectorData;
        public bool ChangeStatus = false;

        public FingerTapEventParameters(FingerTapActionType actionType, FingerTapEvent eventToActivate, GameObject gameData, GameObject selectorData , bool changeStatus = false)
        {
            ActionType = actionType;
            EventtoActivate = eventToActivate;
            GameData = gameData;
            SelectorData = selectorData;
            ChangeStatus = changeStatus;
        }
    }



    /// <summary>
    /// Which Action is being performed, Which event, Target Object and Cursor current position
    /// </summary>
    public UnityEvent<FingerTapEventParameters> OnAction;

    public SkinnedMeshRenderer HandRenderer;

    public Material MaterialForMeshInCursor;
    
    //Obj at finger tip for selection
    public FowardTriggerEvents SelectionCollider;
    //actual collider of the obj
    private Collider _selectionActiveCollider;

    public Transform BaseFingerRaycast;
    public FowardTriggerEvents ConfirmationCollider;

    public FingerTapCursor PrimaryCursor;

    public static float RayDistance = 0.3f;
    private float _defaultDistance;


    public LineRenderer LR;
    private Vector3[] DrawPositions;

    //Change to Vector, will need a refactor for passing position in tap
    private GameObject _nextTapPosition;

    [Space]

    //Dont pass events to the other cursor
    public bool DoNotFowardEvent = false;
    

    private FingerTapAction _currentAction;
    private GameObject _currentTouchedObject;

    private bool _isWaitingTapDelay = false;
    private float _tapDelay = 0.4f;

    //Could be the same, but have different functions so better keep it separate
    private Coroutine _editWaitDeactivateMethod;
    private float _activeEditDelay = 1.5f;


    private GameObject _currentFingerTrackedObject;
    private MeshFilter _selectedMesh = null;

    private bool _hasMarkerLayer = false;
    private int _markerLayer = 0;
    private RaycastHit _currentRaycastHit;
    private GameObject _lastSelectedMarker = null;

    private static ScannedTypeGameObject _newObjectClassification;
    private bool _editIsActive = false;

    public static ScannedTypeGameObject GetCurrentCursorObjClassification() { return _newObjectClassification; }

    private void OnEnable()
    {

        if(_newObjectClassification == null)
        {
            GameObject objClassification = new GameObject("CursorCurrentObjClassification");
            objClassification.transform.SetParent(this.transform);
            _newObjectClassification = objClassification.AddComponent<ScannedTypeGameObject>();
            _newObjectClassification.IgnoreClassificationData = true;
            _newObjectClassification.ObjectClassification = ScannedObjectsClassificationType.NonSelected;
        }

        LR = gameObject.GetComponent<LineRenderer>();
        DrawPositions = new Vector3[2];
        
        _defaultDistance = RayDistance;
        _currentAction =  new FingerTapAction() {  Type = FingerTapActionType.None};
        _currentTouchedObject = null;

        Debug.Assert(SelectionCollider != null);
        Debug.Assert(BaseFingerRaycast != null);
        Debug.Assert(ConfirmationCollider != null);
        Debug.Assert(HandRenderer != null);

        SelectionCollider?.OnTriggerEnterFoward.AddListener(OnFingerEntered);
        SelectionCollider?.OnTriggerExitFoward.AddListener(OnFingerExit);


        FTCursorsEventManager.ReceiveNextTapPosition += SetNextTapPosition;
        FTCursorsEventManager.ChangeAllCursorStatus += ChangeThisCursosStatus;
        FTCursorsEventManager.ExitBtnFTCEffect += ExitButtonEffect;

        FTCursorsEventManager.BasicCursorEventListener += InvokeBasicEvent;
        FTCursorsEventManager.BlockTapsEvent += BlockTaps;
        FTCursorsEventManager.UnblockTapsEvent += UnblockTaps;
        
        FTCursorsEventManager.ChangeCursorRayDistance += ChangeEditDistance;

        _hasMarkerLayer = MarkerManager.HasMarkerLayer();
        if (_hasMarkerLayer)
        {
            int layer = LayerMask.NameToLayer(MarkerManager.MarkerLayer);
            _markerLayer = (1 << layer);
        }

        _selectionActiveCollider = SelectionCollider.GetComponent<Collider>();
        if (_selectionActiveCollider == null)
            _selectionActiveCollider = SelectionCollider.GetComponentInChildren<Collider>();
        if (_selectionActiveCollider == null)
            Debug.LogError("Selection Collider has no collider: FTCursor");
    }

    private void OnDisable()
    {
        SelectionCollider?.OnTriggerEnterFoward.RemoveListener(OnFingerEntered);
        SelectionCollider?.OnTriggerExitFoward.RemoveListener(OnFingerExit);

        FTCursorsEventManager.ChangeAllCursorStatus -= ChangeThisCursosStatus;
        FTCursorsEventManager.ExitBtnFTCEffect -= ExitButtonEffect;
        FTCursorsEventManager.BasicCursorEventListener -= InvokeBasicEvent;

        FTCursorsEventManager.BlockTapsEvent -= BlockTaps;
        FTCursorsEventManager.UnblockTapsEvent -= UnblockTaps;
        FTCursorsEventManager.ChangeCursorRayDistance -= ChangeEditDistance;
        FTCursorsEventManager.ReceiveNextTapPosition -= SetNextTapPosition;
    }
    
    private void SetNextTapPosition(GameObject nextPosition )
    {
        _nextTapPosition = nextPosition;
    }
    
    public void FixedUpdate()
    {
        if(LR.enabled){
        
            Vector3 EndPosition = SelectionCollider.gameObject.transform.position +  ((SelectionCollider.gameObject.transform.position - BaseFingerRaycast.transform.position).normalized * RayDistance );
            
            DrawPositions[0] = new Vector3(SelectionCollider.gameObject.transform.position.x,SelectionCollider.gameObject.transform.position.y,SelectionCollider.gameObject.transform.position.z);
            DrawPositions[1] = new Vector3(EndPosition.x, EndPosition.y,EndPosition.z);
                  
            LR.SetPosition(0, DrawPositions[0]);
            LR.SetPosition(1, DrawPositions[1]);
        }
       
        if(_hasMarkerLayer && MarkerManager.HasMarkers() && !_isWaitingTapDelay)
        {
            if ((_lastSelectedMarker == null)&&
                (Physics.Raycast(SelectionCollider.gameObject.transform.position,
                (SelectionCollider.gameObject.transform.position - BaseFingerRaycast.transform.position).normalized,
                out _currentRaycastHit, RayDistance, _markerLayer)))
            {

                StartCoroutine(WaitTapDelay());
                _lastSelectedMarker = _currentRaycastHit.collider.gameObject;

                OnAction?.Invoke(new FingerTapEventParameters(_currentAction.Type, FingerTapEvent.Select, _currentRaycastHit.collider.gameObject, SelectionCollider.gameObject));
            }
            else if (_lastSelectedMarker!= null)
            {
                _lastSelectedMarker = null;
                OnAction?.Invoke(new FingerTapEventParameters(_currentAction.Type, FingerTapEvent.Deselect, _lastSelectedMarker, SelectionCollider.gameObject));
            }

        }
    }

    private void BlockTaps()
    {
        _isWaitingTapDelay = true;
    }
    private void UnblockTaps()
    {
        _isWaitingTapDelay = false;
    }

    private void ChangeEditDistance(float newDis)
    {
        RayDistance = newDis;
    }

    private void InvokeBasicEvent(FingerTapEvent cursorEvent)
    {
        if (cursorEvent == FingerTapEvent.ActivateEdit )
        {
            //Debug.Log("Esdit activated");
            _editIsActive = true;


            //if already active, reset
            if(_editWaitDeactivateMethod != null )
                StopCoroutine(_editWaitDeactivateMethod);
            _editWaitDeactivateMethod  = StartCoroutine(WaitEditDelay());

            OnAction?.Invoke(new FingerTapEventParameters(_currentAction.Type, cursorEvent, _newObjectClassification?.gameObject, PrimaryCursor.SelectionCollider.gameObject));
            return;
        }

        if (cursorEvent == FingerTapEvent.Tap)
            OnTap();
        else
            OnAction?.Invoke(new FingerTapEventParameters(_currentAction.Type, cursorEvent, _newObjectClassification?.gameObject, PrimaryCursor.SelectionCollider.gameObject));
    }

    private void ExitButtonEffect(FingerTapActionType newStatus, Collider newshape)
    {
        OnFingerExit( newshape, new FingerTapAction(){ Type = newStatus});
    }

    private void ChangeThisCursosStatus(FingerTapActionType newStatus, Color newColor, ScannedObjectsClassificationType newType = ScannedObjectsClassificationType.NonSelected)
    {

        _newObjectClassification.ObjectClassification = newType;

        ChangeEditDistance(_defaultDistance);
        //Cancel pending operation
        OnAction?.Invoke(new FingerTapEventParameters(_currentAction.Type, FingerTapEvent.Cancel, _currentFingerTrackedObject != null ? _currentFingerTrackedObject : _currentTouchedObject, SelectionCollider.gameObject));
        //Clears cursor object
        TrackObjectOnFinger(null);

        _currentAction = new FingerTapAction() {  Type=newStatus};
        HandRenderer.material.SetColor("_ColorTop", newColor)  ;
    }

    #region FingerEvents
    //Reaaaaally need to find a better name for these events
    private void OnFingerEntered(Collider touchedObject, FingerTapAction selectedAction)
    {
        //Indexes touch each other, cancels
        if(selectedAction.Type == FingerTapActionType.IndexPoint && touchedObject != SelectionCollider.gameObject && touchedObject != ConfirmationCollider.gameObject)
            OnAction?.Invoke(new FingerTapEventParameters(_currentAction.Type, FingerTapEvent.Undo, _currentFingerTrackedObject != null ? _currentFingerTrackedObject : _currentTouchedObject, SelectionCollider.gameObject));

        if (_isWaitingTapDelay)
            return;

        if (selectedAction.Type == FingerTapActionType.OtherObject ) 
        {
            _currentTouchedObject = touchedObject.gameObject;
            _selectedMesh = _currentTouchedObject.GetComponent<MeshFilter>();
            if (_selectedMesh != null)
            {
                //if edit is runing, increase wait time
                if(_editWaitDeactivateMethod == null)
                    StartCoroutine(WaitTapDelay());
                else
                    StartCoroutine(WaitTapDelay(_activeEditDelay)); 


                OnAction?.Invoke(new FingerTapEventParameters(_currentAction.Type, FingerTapEvent.Select, _currentTouchedObject, SelectionCollider.gameObject, _editIsActive));
            }
        }
    }
    private void OnFingerExit(Collider touchedObject, FingerTapAction selectedAction)
    {
        if (selectedAction.Type == FingerTapActionType.OtherObject )//&& _currentAction.Type == FingerTapActionType.None)
        {
            StartCoroutine(Deselect());
        }

        _currentTouchedObject = null;

        //Starts tracking object for creation at the finger cursor
        if (selectedAction.Type == FingerTapActionType.AddMesh && !DoNotFowardEvent)
        {
            AddMeshToCursor(touchedObject);
            return;
        }

        //clears finger cursor
        if (selectedAction.Type != FingerTapActionType.OtherObject && selectedAction.Type != FingerTapActionType.ConfirmPoint && selectedAction.Type != FingerTapActionType.IndexPoint)
            TrackObjectOnFinger(null);
    }

    #endregion

    #region ConfirmationEvents
    /*
    private void OnConfirmationEntered(Collider touchedObject, FingerTapAction selectedAction)
    {
        //Debug.Log("Confirmation sent touching "+_currentTouchedObject);

        //same hand finger touched it
        if (touchedObject?.gameObject == _selectionActiveCollider.gameObject)
        {
            if (!_isWaitingTapDelay)
            {
                StartCoroutine(WaitTapDelay());
                

                //Confirm action generates an tap is on primary if secondary cursor
                if (PrimaryCursor != null)
                {
                    PrimaryCursor.OnAction?.Invoke(_currentAction.Type, FingerTapEvent.SecondaryTap, PrimaryCursor.GetCurrentIndexSelectedObj(), PrimaryCursor.SelectionCollider.gameObject);
                }
            }
        }
        else
        {   //original tap, touching thumb
            if (!_isWaitingTapDelay )
            {
                StartCoroutine(WaitTapDelay());
                OnAction?.Invoke(_currentAction.Type, FingerTapEvent.Tap, this.GetCurrentIndexSelectedObj(), SelectionCollider.gameObject);
            }
        }
    }

    private void OnConfirmationExit(Collider touchedObject, FingerTapAction selectedAction)
    {
        //Debug.Log(touchedObject);
        //Debug.Log(selectedAction);

        //_isHoldingConfirmation = false;
        //StartCoroutine(OnCancelHold());
        OnAction?.Invoke(_currentAction.Type, FingerTapEvent.Release, _currentTouchedObject, SelectionCollider.gameObject);
    }
    */
    #endregion

    private void OnTap()
    {
        if (!_isWaitingTapDelay)
        {
            StartCoroutine(WaitTapDelay());
            //Confirm action generates an tap is on primary if secondary cursor
            if (PrimaryCursor != null)
            {
                PrimaryCursor.OnAction?.Invoke(new FingerTapEventParameters(_currentAction.Type, FingerTapEvent.Tap, PrimaryCursor.GetCurrentIndexSelectedObj(), _nextTapPosition==null? PrimaryCursor.SelectionCollider.gameObject:_nextTapPosition));
                _nextTapPosition = null;
            }
        }
    }

    private void AddMeshToCursor(Collider colliderToAdd)
    {

        if ( colliderToAdd.GetType() == typeof(BoxCollider))
        {
            var baseCube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            baseCube.GetComponent<Renderer>().material = MaterialForMeshInCursor;
            baseCube.transform.parent = null;            
            baseCube.name = "Cube OnTap";
            baseCube.transform.localPosition = Vector3.zero;

            TrackObjectOnFinger(baseCube);
        }
    }

    public void TrackObjectOnFinger(GameObject objectToTrack)
    {

        if (_currentFingerTrackedObject != null)
            Destroy(_currentFingerTrackedObject);

        objectToTrack?.transform.SetParent(SelectionCollider.gameObject.transform, false);
        _currentFingerTrackedObject = objectToTrack;
    }
    public GameObject GetCurrentIndexSelectedObj()
    {
        return _currentFingerTrackedObject != null ? _currentFingerTrackedObject : _currentTouchedObject;
    }

    private IEnumerator Deselect()
    {
        yield return null;
        yield return new WaitUntil(() => !_isWaitingTapDelay);

        OnAction?.Invoke(new FingerTapEventParameters(_currentAction.Type, FingerTapEvent.Deselect, GetCurrentIndexSelectedObj(), SelectionCollider.gameObject));
    }

    private IEnumerator WaitTapDelay(float customdelay = -10f)
    {
        _isWaitingTapDelay = true;
        if(customdelay < -5)
            yield return new WaitForSeconds(_tapDelay);
        else
            yield return new WaitForSeconds(customdelay);
        _isWaitingTapDelay = false;
    }

    private IEnumerator WaitEditDelay()
    {
        yield return new WaitForSeconds(_activeEditDelay);
        _editIsActive = false;
        _editWaitDeactivateMethod = null;
    }


}
