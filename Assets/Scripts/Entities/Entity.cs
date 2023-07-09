using UnityEngine;
using System.Threading.Tasks;
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
    [HideInInspector] public Entity CurrentHolder;
    [HideInInspector] public Entity CurrentHeld;

    public Animator Animator;
    public SpriteRenderer SpriteRenderer;
    public Collider2D Collider;
    public Rigidbody2D Rigidbody;

    public Transform HeldPivot;
    public GameObject Canvas;
    public TextMeshProUGUI DialogLabel;

    public float MoveSpeed;
    public EntityState State;

    private IEnumerator returnToUserCoroutine;

    public static Action OnEntityHitAction;

    
    void Update () {
        switch ( State ) {
            case EntityState.Attacking:
            case EntityState.Defending:
            case EntityState.Refereeing:
                if ( Rigidbody.velocity.magnitude > 0 )
                    Animator.Play ( stateName: "human_run" );
                else
                    Animator.Play ( stateName: "human_idle" );

                break;

            case EntityState.Held:
                if ( Main.IsWeaponMidAir )
                    Animator.Play ( stateName: "weapon_fly" );
                else
                    Animator.Play ( stateName: "weapon_idle" );

                break;
        }
    }

    public void ChangeState ( EntityState newState ) {
        State = newState;

        switch ( State ) {
            case EntityState.Attacking:
                Collider.enabled = false;

                gameObject.GetComponent<Hittable> ().enabled = true;
                gameObject.GetComponent<Hitter> ().enabled = false;
                break;
            
            case EntityState.Defending:
                Collider.enabled = true;

                gameObject.GetComponent<Hittable> ().enabled = true;
                gameObject.GetComponent<Hitter> ().enabled = false;

                break;
            
            case EntityState.Held:
                Collider.enabled = true;

                gameObject.GetComponent<Hittable> ().enabled = false;
                gameObject.GetComponent<Hitter> ().enabled = true;

                break;
            
            case EntityState.Refereeing:
                Collider.enabled = true;

                gameObject.GetComponent<Hittable> ().enabled = true;
                gameObject.GetComponent<Hitter> ().enabled = false;

                break;
        }
    }

    public async void ShowDialog ( String dialogText, int millisecondsDelay ) {
        DialogLabel.text = dialogText;

        Canvas.SetActive ( true );

        await Task.Delay( millisecondsDelay: millisecondsDelay );

        Canvas.SetActive ( false );
    }



    public void Throw ( Vector2 startDirection ) {
        if ( HeldPivot.childCount > 0 ) {
            CurrentHeld = HeldPivot.GetChild ( 0 ).GetComponent<Entity> ();
            
            CurrentHeld.OnThrow (
                holder: this,
                startDirection: startDirection,
                throwDirection: Main.throwDirection
            );

            CurrentHeld = null;
        }
    }


    public async void OnThrow ( Entity holder, Vector2 startDirection, Vector2 throwDirection ) {
        CurrentHolder = holder;
                    
        Main.IsWeaponMidAir = true;

        transform.SetParent ( FindObjectOfType<Main> ().GameEntities.transform );
        
        Rigidbody.AddForce ( throwDirection * MoveSpeed, ForceMode2D.Impulse );
        Rigidbody.AddForce ( startDirection * MoveSpeed / 2, ForceMode2D.Impulse );

        await Task.Delay ( millisecondsDelay: 500 );

        returnToUserCoroutine = ReturnToUser ();
        StartCoroutine( returnToUserCoroutine );
    }
   


    public void Catch () {}


    public void OnCatch () {
        CurrentHolder.Catch ();
        
        Rigidbody.velocity = Vector2.zero;

        transform.SetParent ( CurrentHolder.HeldPivot );

        transform.localPosition = Vector3.zero;
    }



    public void Hit ( Entity hit ) {
        ChangeState ( EntityState.Attacking );

        if ( returnToUserCoroutine != null ) 
            StopCoroutine ( returnToUserCoroutine );
        
        Rigidbody.velocity = Vector2.zero;

        transform.position = hit.transform.position;
        transform.rotation = Quaternion.identity;
        transform.localScale = new Vector3 ( Mathf.Sign ( hit.transform.lossyScale.x ) * Mathf.Abs ( transform.lossyScale.x ), transform.lossyScale.y, transform.lossyScale.z );
    }


    public void OnHit ( Entity hit ) {
        if ( State == EntityState.Attacking ) {
            ChangeState ( EntityState.Defending );
        }
        else {
            ChangeState ( EntityState.Held );

            transform.SetParent ( hit.HeldPivot );

            transform.localPosition = Vector3.zero;
        }
    }


    void OnTriggerEnter2D ( Collider2D collided ) {
        if ( State == EntityState.Held ) {
            Entity hitEntity = collided.GetComponent<Entity> ();
            
            if ( hitEntity != null ) {
                if ( hitEntity.State == EntityState.Defending || hitEntity.State == EntityState.Refereeing ) {
                    Animator.Play ( stateName: "hit" );
                    hitEntity.Animator.Play ( stateName: "hit" );

                    OnEntityHitAction?.Invoke ();

                    Hit ( hit: hitEntity );
                    hitEntity.OnHit ( hit: this );
                    CurrentHolder.OnHit ( hit: this );

                    Main.IsWeaponMidAir = false;
                    CurrentHolder = null;
                }
            }
        }
    }



    private IEnumerator ReturnToUser () {
        bool isNearHolder() => Mathf.Abs ( ( transform.position - CurrentHolder.transform.position ).magnitude ) <= 1;
        Vector2 startDirection = Rigidbody.velocity.normalized;
        float directionTValue = 0;

        while ( !isNearHolder() ) {
            directionTValue += 0.01f;

            Vector2 returnDirection = Vector3.Lerp ( startDirection, ( CurrentHolder.transform.position - transform.position ).normalized, directionTValue );

            Rigidbody.AddForce( returnDirection * MoveSpeed, ForceMode2D.Force );

            yield return null;
        }

        OnCatch ();
    }
}
