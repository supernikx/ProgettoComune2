﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StickyObject : MonoBehaviour, IPoolObject
{
    #region IPoolObject
    public GameObject ownerObject
    {
        get
        {
            return _ownerObject;
        }
        set
        {
            _ownerObject = value;
        }
    }
    GameObject _ownerObject;

    public State CurrentState
    {
        get
        {
            return _currentState;
        }

        set
        {
            _currentState = value;
        }
    }
    State _currentState;

    public event PoolManagerEvets.Events OnObjectSpawn;
    public event PoolManagerEvets.Events OnObjectDestroy;
    #endregion

    [Header("Stickyobject General Settings")]
    [SerializeField]
    private float duration;
    [SerializeField]
    private BoxCollider boxCollider;
    private Bounds stickyBounds;
    [Header("Stickyobject Range Settings")]
    [SerializeField]
    private float minRange;
    [SerializeField]
    private int checkSpaceRayCount = 4;
    private float checkSpaceRaySpacing;
    private float checkSpaceRayLenght = 0.3f;
    [SerializeField]
    private LayerMask stickyObjectCollisionLayer;
    [SerializeField]
    private LayerMask avoidStickyObjectLayer;
    [SerializeField]
    private Transform rightPoint;
    [SerializeField]
    private Transform leftPoint;

    [Header("Stickyobject Stuck Settings")]
    [SerializeField]
    private int checkStuckedObjectsRayCount;
    private float checkStuckedObjectsRaySpacing;
    [SerializeField]
    private int horizontalRayRequiredStuckObject;
    [SerializeField]
    private int verticalRayRequiredStuckObject;
    [SerializeField]
    private LayerMask layersWhichCanBeStucked;
    float maxDistance;
    float maxXScale;
    float yOffset;

    [Header("Stickyobject Graphic Settings")]
    [SerializeField]
    private GameObject pivotFix;
    [SerializeField]
    private GameObject graphic;
    [SerializeField]
    private ParticleSystem spawnParticle;

    Vector3 rightMaxPosition;
    Vector3 leftMaxPosition;

    #region API
    /// <summary>
    /// Funzione di Setup
    /// </summary>
    public void Setup()
    {
        stickyBounds = boxCollider.bounds;
        Vector3 bottomLeftCorner = new Vector3(stickyBounds.min.x, stickyBounds.min.y, transform.position.z);
        Vector3 bottomRightCorner = new Vector3(stickyBounds.max.x, stickyBounds.min.y, transform.position.z);
        yOffset = stickyBounds.size.y * 1f;
        maxDistance = Vector3.Distance(bottomLeftCorner, bottomRightCorner);
        maxXScale = transform.localScale.x;
        checkSpaceRaySpacing = CalculateRaySpacing(checkSpaceRayCount);
    }

    /// <summary>
    /// Funzione di inizializzazione
    /// </summary>
    /// <param name="_spawnPosition"></param>
    /// <param name="_rotation"></param>
    public bool Init(Vector3 _spawnPosition, Vector3 _normal, Quaternion _rotation)
    {
        transform.position = _spawnPosition;
        Vector3 positionToCheck = new Vector3(0, checkSpaceRayLenght - (checkSpaceRayLenght / 3f), 0);
        pivotFix.transform.localPosition = positionToCheck;
        transform.rotation = _rotation;

        rightMaxPosition = CheckSpace(_normal, 1);
        leftMaxPosition = CheckSpace(_normal, -1);

        float actualDistance = Vector3.Distance(leftMaxPosition, rightMaxPosition);
        pivotFix.transform.localPosition = Vector3.zero;
        if (actualDistance < minRange)
        {
            if (OnObjectDestroy != null)
                OnObjectDestroy(this);

            return false;
        }
        else
        {
            graphic.SetActive(false);
            return true;
        }
    }

    /// <summary>
    /// Funzione che fa spawnare lo sticky object (da chiamare dopo Init)
    /// </summary>
    public void Spawn()
    {
        float actualDistance = Vector3.Distance(leftMaxPosition, rightMaxPosition);
        Vector3 newScale = new Vector3(actualDistance * maxXScale / maxDistance, transform.localScale.y, transform.localScale.z);
        transform.localScale = newScale;
        boxCollider.size.Scale(newScale);
        transform.position = Vector3.Lerp(leftMaxPosition, rightMaxPosition, 0.5f);
        leftPoint.position = leftMaxPosition;
        rightPoint.position = rightMaxPosition;
        velocity = Vector3.zero;
        previousPosition = transform.position;
        checkStuckedObjectsRaySpacing = CalculateRaySpacing(checkStuckedObjectsRayCount);

        graphic.SetActive(true);
        spawnParticle.Play(true);

        if (OnObjectSpawn != null)
            OnObjectSpawn(this);

        StartCoroutine(DespawnCoroutine());
    }

    /// <summary>
    /// Funzione che controlla e manda gli eventi di Sticky/StickyEnd
    /// </summary>
    public void StickyBehaviour()
    {
        //Calcolo lo spazio tra un ray e l'altro
        if (CurrentState == State.InPool)
            return;

        Direction hitDirection = GetHitDirection();
        int rayRequiredStuckObject = (hitDirection == Direction.Above || hitDirection == Direction.Below) ? horizontalRayRequiredStuckObject : verticalRayRequiredStuckObject;

        //Determina la lunghezza del raycast
        float rayLenght = 0.1f;

        //Ciclo tutti i ray
        for (int i = 0; i < checkStuckedObjectsRayCount; i++)
        {
            //Determina il punto da cui deve partire il ray
            Vector3 rayOrigin = leftPoint.position;
            rayOrigin += transform.right.normalized * (checkStuckedObjectsRaySpacing * i);

            //Imposto i parametri per il linecast
            RaycastHit hit;
            Ray ray = new Ray(rayOrigin, transform.up.normalized);
            Vector3 endPosition = rayOrigin + transform.up.normalized * rayLenght;

            //Eseguo il linecast
            if (Physics.Raycast(ray, out hit, rayLenght, layersWhichCanBeStucked))
            {
                Debug.DrawLine(rayOrigin, endPosition, Color.red);

                //Se colpisco qualcosa che rientra nei layer stuck
                GameObject objectHit = hit.transform.gameObject;

                if ((objectHit.layer == LayerMask.NameToLayer("Player") || objectHit.layer == LayerMask.NameToLayer("PlayerImmunity")) && objectHit.transform.parent != null)
                {
                    objectHit = objectHit.transform.parent.gameObject;
                }

                if (tempImmunityObjectList.Contains(objectHit))
                    continue;

                StickyCollisionsInfos stickyInfo = stickyList.FirstOrDefault(g => g.Gobject == objectHit);

                //Controllo se è presente nella lista
                if (stickyInfo == null)
                {
                    //Se non è presente controllo che sia un oggetto ISticky
                    ISticky stickyRef = objectHit.GetComponent<ISticky>();

                    if (stickyRef != null)
                    {
                        //Se lo è creo un nuovo StickyCollisionsInfos con i dati del GameObject e lo aggiunto alla lista
                        stickyInfo = new StickyCollisionsInfos(objectHit, stickyRef);
                        stickyInfo.StickyRay++;
                        stickyList.Add(stickyInfo);
                    }
                }
                else
                {
                    //Se è resente aumento i ray che lo colpiscono di uno
                    stickyInfo.StickyRay++;
                }
            }

            Debug.DrawLine(rayOrigin, endPosition, Color.blue);
        }

        //Calcolo la velocity
        CalculateVelocity();

        //Ciclo tutti gli oggetti con cui sono in collisione
        for (int i = stickyList.Count - 1; i >= 0; i--)
        {
            //Se i ray che colpiscono l'oggetto sono maggiori di quelli necessari e non è ancora incolllato
            if (stickyList[i].StickyRay >= rayRequiredStuckObject && !stickyList[i].Sticky)
            {
                //Lo incollo chiamo la callback comunicando la direzione in cui si è bloccati
                stickyList[i].Sticky = true;
                stickyList[i].GIStickyRef.OnSticky(GetHitDirection());
            }
            //Se i ray che colpiscono l'oggetto sono minori di quelli necessari e sono incollato
            else if (stickyList[i].StickyRay < rayRequiredStuckObject && stickyList[i].Sticky)
            {
                //Lo scollo chiamo la callback di fine sticky
                stickyList[i].Sticky = false;
                stickyList[i].GIStickyRef.OnStickyEnd();
                StartCoroutine(DelayTempListCoroutine(stickyList[i].Gobject));
            }
            //Se i ray che colpiscono l'oggetto sono 0
            else if (stickyList[i].StickyRay == 0)
            {
                //Rimuovo l'oggetto dalla lista
                stickyList.RemoveAt(i);
                continue;
            }

            //Muovo l'oggetto attaccato se velocity > 0
            if (!float.IsNaN(velocity.x) && velocity != Vector3.zero)
                MovePassenger(stickyList[i].Gobject.transform, velocity * Time.deltaTime);

            //Azzero i ray che colpiscono l'oggetto
            stickyList[i].StickyRay = 0;
        }
    }

    /// <summary>
    /// Funzione che calcola ritorna l'ultimo punto con cui si collide con il collision layer
    /// nella direzione passata come parametro
    /// </summary>
    /// <param name="_stickyObject"></param>
    /// <param name="_normal"></param>
    /// <param name="_direction"></param>
    /// <returns></returns>
    public Vector3 CheckSpace(Vector3 _normal, int _direction)
    {
        Vector3 maxCenterPoint = transform.position + transform.up.normalized * yOffset;
        Vector3 previewRayOrigin = maxCenterPoint;

        for (int i = 0; i < checkSpaceRayCount; i++)
        {
            //Determina il punto da cui deve partire il ray
            Vector3 rayOrigin = maxCenterPoint;
            rayOrigin += transform.right * _direction * (checkSpaceRaySpacing * 0.5f * i);

            //Crea il ray
            Ray ray = new Ray(rayOrigin, -transform.up);
            RaycastHit hit;

            //Eseguo il raycast
            if (Physics.Raycast(ray, out hit, checkSpaceRayLenght, avoidStickyObjectLayer))
            {
                if (hit.transform.gameObject != gameObject)
                    break;
            }

            //Eseguo il raycast
            if (Physics.Raycast(ray, out hit, checkSpaceRayLenght, stickyObjectCollisionLayer))
            {
                previewRayOrigin = rayOrigin;
            }
            else
            {
                break;
            }
        }

        return previewRayOrigin;
    }

    #region Getter
    /// <summary>
    /// Funzione che ritorna la durata dell'oggetto sticky
    /// </summary>
    /// <returns></returns>
    public float GetDuration()
    {
        return duration;
    }

    /// <summary>
    /// Funzione che ritorna il collision layer dell'ggetto sticky
    /// </summary>
    /// <returns></returns>
    public LayerMask GetCollisionLayer()
    {
        return stickyObjectCollisionLayer;
    }
    #endregion
    #endregion

    /// <summary>
    /// Funzione che calcola lo spazio tra i raycast orrizontali con il valore di ray passato come parametro
    /// </summary>
    /// <param name="_rayCount"></param>
    /// <returns></returns>
    private float CalculateRaySpacing(float _rayCount)
    {
        if (boxCollider.bounds.size.x > boxCollider.bounds.size.y)
            return boxCollider.bounds.size.x / (_rayCount - 1);
        else
            return boxCollider.bounds.size.y / (_rayCount - 1);
    }

    /// <summary>
    /// Funzione che conta il tempo per far despawnare l'oggetto
    /// </summary>
    /// <returns></returns>
    private IEnumerator DespawnCoroutine()
    {
        yield return new WaitForSeconds(duration);

        for (int i = 0; i < stickyList.Count; i++)
        {
            stickyList[i].GIStickyRef.OnStickyEnd();
        }
        stickyList.Clear();
        graphic.SetActive(true);

        if (OnObjectDestroy != null)
            OnObjectDestroy(this);
    }

    /// <summary>
    /// Classe che contiene le informazioni degli oggetti che sono in contatto con l'oggetto sticky
    /// </summary>
    private class StickyCollisionsInfos
    {
        public GameObject Gobject;
        public ISticky GIStickyRef;
        public int StickyRay;
        public bool Sticky;

        public StickyCollisionsInfos(GameObject _Gobject, ISticky _GIStickyRef)
        {
            Gobject = _Gobject;
            GIStickyRef = _GIStickyRef;
            StickyRay = 0;
            Sticky = false;
        }
    }
    /// <summary>
    /// Funzione che controlla le collisioni con oggetti ISticky
    /// </summary>
    /// <param name="_rightPosition"></param>
    /// <param name="_leftPostion"></param>
    /// <returns></returns>
    private List<StickyCollisionsInfos> stickyList = new List<StickyCollisionsInfos>();

    /// <summary>
    /// Funzione che sposta eventuali oggetti attaccati
    /// </summary>
    /// <param name="_hitTransform"></param>
    /// <param name="_velocity"></param>
    private void MovePassenger(Transform _hitTransform, Vector3 _velocity)
    {
        HashSet<Transform> movedPassengers = new HashSet<Transform>();
        float directionX = Mathf.Sign(_velocity.x);
        float directionY = Mathf.Sign(_velocity.y);

        //Vertical Moving platform
        if (_velocity.y != 0)
        {
            if (!movedPassengers.Contains(_hitTransform))
            {
                movedPassengers.Add(_hitTransform);

                float pushX = (directionY == 1) ? _velocity.x : 0;
                float pushY = _velocity.y; //(directionY == 1) ? _velocity.y : _velocity.y - collisionOffset;

                _hitTransform.Translate(new Vector3(pushX, pushY, 0));
            }
        }

        //Horizontal Moving platform
        if (_velocity.x != 0)
        {
            if (!movedPassengers.Contains(_hitTransform))
            {
                movedPassengers.Add(_hitTransform);

                float pushX = _velocity.x;
                float pushY = 0f;

                PlayerCollisionController collisionCtrl = _hitTransform.GetComponentInParent<PlayerCollisionController>();
                if (collisionCtrl != null)
                {
                    collisionCtrl.transform.Translate(collisionCtrl.CheckMovementCollisions(new Vector3(pushX, pushY, 0)));
                }
                else
                {
                    _hitTransform.Translate(new Vector3(pushX, pushY, 0));
                }
            }
        }

        //Passenger Fix horzizontal top e piattaforma che scende
        if (directionY == -1 || _velocity.y == 0 && _velocity.x != 0)
        {
            if (!movedPassengers.Contains(_hitTransform))
            {
                movedPassengers.Add(_hitTransform);

                float pushX = _velocity.x;
                float pushY = _velocity.y;

                PlayerCollisionController collisionCtrl = _hitTransform.GetComponentInParent<PlayerCollisionController>();
                if (collisionCtrl != null)
                {
                    collisionCtrl.transform.Translate(collisionCtrl.CheckMovementCollisions(new Vector3(pushX, pushY, 0)));
                }
                else
                {
                    _hitTransform.Translate(new Vector3(pushX, pushY, 0));
                }
            }
        }
    }

    /// <summary>
    /// Funzione che mette in una "black list" gli oggetti che si sono appena staccati per 0.1 secondi
    /// </summary>
    static List<GameObject> tempImmunityObjectList = new List<GameObject>();
    private IEnumerator DelayTempListCoroutine(GameObject _objectToAdd)
    {
        GameObject objectToAdd = _objectToAdd;
        tempImmunityObjectList.Add(objectToAdd);
        yield return new WaitForSeconds(0.5f);
        tempImmunityObjectList.Remove(objectToAdd);
    }

    #region Velocity
    /// <summary>
    /// Vettore di velocità del player
    /// </summary>
    Vector3 velocity;
    /// <summary>
    /// Poszione al frame precendente del player
    /// </summary>
    Vector3 previousPosition;
    /// <summary>
    /// Funzion che calcola la velocità di movimento
    /// </summary>
    void CalculateVelocity()
    {
        if (Vector3.Distance(transform.position, previousPosition) < 10f)
            velocity = (transform.position - previousPosition) / Time.deltaTime;
        previousPosition = transform.position;
    }
    #endregion

    /// <summary>
    /// Funzione che ritorna la direzione opposta alla posizione attuale dell'oggetto
    /// </summary>
    /// <returns></returns>
    private Direction GetHitDirection()
    {
        if (transform.right.y > 0.5f)
            return Direction.Right;
        if (transform.right.y < -0.5f)
            return Direction.Left; ;
        if (transform.right.x > 0.5f)
            return Direction.Below;
        if (transform.right.x < -0.5f)
            return Direction.Above;

        return Direction.None;
    }

    private void OnDisable()
    {
        OnObjectSpawn = null;
        OnObjectDestroy = null;
    }
}
