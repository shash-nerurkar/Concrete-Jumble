using UnityEngine;
using System.Collections;
using System;
using TMPro;

public enum EntityState {
    Attacking,
    Defending,
    Refereeing,
    Held,
}

public class Entity : MonoBehaviour
{
    public Material baseMaterial;
    public Material weaponMaterial;

    public Main main;
    public HUD hud;
    public SoundsMain soundsMain;

    public Entity CurrentHolder;
    public Entity CurrentHeld;

    public Animator Animator;
    public SpriteRenderer SpriteRenderer;
    public Collider2D HitterCollider;
    public Collider2D HittableCollider;
    public Rigidbody2D Rigidbody;

    public Transform HeldPivot;
    public GameObject Canvas;
    public TextMeshProUGUI DialogLabel;
    public Timer returnToHolderTimer;


    public Vector3 Velocity;
    public float MoveSpeed;
    public float FlySpeed;
    public EntityState State;
    private bool isHit = false;
    private IEnumerator returnToUserCoroutine;

    
    void Update () {
        if ( !isHit )
            switch ( State ) {
                case EntityState.Attacking:
                case EntityState.Defending:
                case EntityState.Refereeing:
                    Animator.Play ( stateName: "human_run" );
                    break;

                case EntityState.Held:
                    if ( Rigidbody.velocity.magnitude > 0 )
                        Animator.Play ( stateName: "weapon_fly" );
                    else
                        Animator.Play ( stateName: "weapon_idle" );

                    break;
            }
    }

    public void ChangeState ( EntityState newState ) {
        State = newState;

        Hittable hittable = gameObject.GetComponent<Hittable> ();
        Hitter hitter = gameObject.GetComponent<Hitter> ();

        switch ( State ) {
            case EntityState.Attacking:
            case EntityState.Defending:
            case EntityState.Refereeing:
                if ( hittable is Player )
                    ( hittable as Player ).EnableActions ();
                else
                    hittable.enabled = true;
                hitter.enabled = false;

                SpriteRenderer.material = baseMaterial;

                break;
            
            case EntityState.Held:
                if ( hittable is Player )
                    ( hittable as Player ).EnableActions ();
                else
                    hittable.enabled = false;
                gameObject.GetComponent<Hitter> ().enabled = true;

                SpriteRenderer.material = weaponMaterial;

                break;
        }
    }

    public void SetColliderStatus () {
        switch ( State ) {
            case EntityState.Attacking:   
                HittableCollider.enabled = false;
                HitterCollider.enabled = false;
                break;

            case EntityState.Defending:   
            case EntityState.Refereeing:    
                HittableCollider.enabled = true;
                HitterCollider.enabled = false;
                break;
           
            case EntityState.Held:   
                HittableCollider.enabled = false;
                HitterCollider.enabled = true;
                break;
        }
    }

    public void ToggleDialog ( bool shouldShow, String dialogText ) {
        if ( shouldShow ) {
            DialogLabel.text = dialogText;

            Canvas.SetActive ( true );
        }
        else {
            Canvas.SetActive ( false );
        }
    }

    public void Reset ( Transform parent, EntityState state, Vector3 position, Entity CurrentHolder, Entity CurrentHeld ) {
        transform.SetParent ( parent );
        transform.localPosition = position;

        ChangeState ( state );
        SetColliderStatus ();

        Canvas.SetActive ( false );

        Velocity = Vector3.zero;

        Rigidbody.velocity = Vector2.zero;

        this.CurrentHolder = CurrentHolder;
        this.CurrentHeld = CurrentHeld;

        if ( returnToHolderTimer != null && returnToHolderTimer.IsRunning )
            returnToHolderTimer.PauseTimer ();
        if ( returnToUserCoroutine != null )
            StopCoroutine ( returnToUserCoroutine );

        isHit = false;
    }


    public void Throw ( Vector2 startDirection ) {
        if ( HeldPivot.childCount > 0 ) {
            soundsMain.onEntityThrow.Play ();
            
            startDirection.Scale ( new Vector3 ( 0, 0.5f ) );

            CurrentHeld.OnThrow ( holder: this, startDirection: startDirection, throwDirection: Main.throwDirection );
        }
    }

    public void OnThrow ( Entity holder, Vector2 startDirection, Vector2 throwDirection ) {
        transform.SetParent ( main.GameEntities.transform );
        
        Rigidbody.AddForce ( throwDirection * FlySpeed, ForceMode2D.Impulse );
        Rigidbody.AddForce ( startDirection * FlySpeed / 2, ForceMode2D.Impulse );

        returnToHolderTimer.StartTimer ( maxTime: 0.5f, onTimerFinish: StartReturnToUserCoroutine );
    }
   
    public void Catch () {}

    public void OnCatch () {
        CurrentHolder.Catch ();
        
        Rigidbody.velocity = Vector2.zero;

        transform.SetParent ( CurrentHolder.HeldPivot );

        transform.localPosition = Vector3.zero;
    }

    public void Hit ( Entity hit ) { 
        transform.position = hit.transform.position;
        transform.rotation = Quaternion.identity;
    }

    public void OnHit ( Entity hit, EntityState prevState, Entity hitCurrentHolder ) {
        if ( prevState == EntityState.Defending )
            transform.SetParent ( hit.HeldPivot );
        else
            transform.SetParent ( hitCurrentHolder.HeldPivot );

        transform.localPosition = Vector3.zero;
    }



    void OnTriggerEnter2D ( Collider2D collided ) {
        if ( State != EntityState.Held )
            return;

        if ( !HitterCollider.isActiveAndEnabled )
            return;

        Entity hitEntity = collided.GetComponent<Entity> ();
        if ( hitEntity == null )
            return;
                

        isHit = true;
        hitEntity.isHit = true;


        HitterCollider.enabled = false;
        hitEntity.HittableCollider.enabled = false;


        if ( returnToHolderTimer.IsRunning )
            returnToHolderTimer.PauseTimer ();
        if ( returnToUserCoroutine != null )
            StopCoroutine ( returnToUserCoroutine );


        if ( hitEntity.State == EntityState.Defending )
            main.ChangeThrowDirection ( throwDir: Vector2.zero );


        Animator.Play ( stateName: "hit" );
        hitEntity.Animator.Play ( stateName: "hit" );


        Rigidbody.velocity = Vector2.zero;


        soundsMain.onEntityHit.Play ();
        

        EntityState hitEntityState = hitEntity.State;
        Entity hitCurrentHolder = CurrentHolder;
        if ( hitEntity.State == EntityState.Defending ) {
            ChangeState ( EntityState.Attacking );
            hitEntity.ChangeState ( EntityState.Held );
            CurrentHolder.ChangeState ( EntityState.Defending );

            CurrentHolder.CurrentHeld = null;
            CurrentHolder = null;

            CurrentHeld = hitEntity;
            hitEntity.CurrentHolder = this;
        }
        else if ( hitEntity.State == EntityState.Refereeing ) {
            ChangeState ( EntityState.Refereeing );
            hitEntity.ChangeState ( EntityState.Held );
            CurrentHolder.ChangeState ( EntityState.Attacking );

            CurrentHolder.CurrentHeld = hitEntity;
            hitEntity.CurrentHolder = CurrentHolder;

            CurrentHolder = null;
        }

                    
        hud.RoleReversalPanel.ReverseRoles ( entity: this, hitEntity: hitEntity, hitEntityState: hitEntityState, entityCurrentHolder: hitCurrentHolder );
    }
    
    public void OnRoleReversalDone ( Entity hitEntity, EntityState hitEntityState, Entity hitCurrentHolder ) {
        Hit ( hit: hitEntity );
        hitEntity.OnHit ( hit: this, prevState: hitEntityState, hitCurrentHolder: hitCurrentHolder );

        SetColliderStatus ();
        hitEntity.SetColliderStatus ();
        hitCurrentHolder.SetColliderStatus ();

        isHit = false;
        hitEntity.isHit = false;
    }



    void StartReturnToUserCoroutine () {
        returnToUserCoroutine = ReturnToUser ();
        StartCoroutine( returnToUserCoroutine );
    }

    private IEnumerator ReturnToUser () {
        bool isNearHolder() => Mathf.Abs ( ( transform.position - CurrentHolder.transform.position ).magnitude ) <= 0.3f;
        Vector2 startDirection = Rigidbody.velocity.normalized;
        float directionTValue = 0;

        while ( !isNearHolder() ) {
            directionTValue += 0.01f;

            Vector2 returnDirection = Vector3.Lerp ( startDirection, ( CurrentHolder.transform.position - transform.position ).normalized, directionTValue );

            Rigidbody.AddForce( returnDirection * FlySpeed, ForceMode2D.Force );

            yield return null;
        }

        OnCatch ();
    }
}
