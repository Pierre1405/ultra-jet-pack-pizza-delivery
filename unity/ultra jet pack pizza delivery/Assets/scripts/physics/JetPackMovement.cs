using System.Collections;
using System.Collections.Generic;
using Assets.scripts.physics;
using UnityEngine;
using UnityEngine.InputSystem;
using static Assets.scripts.physics.JumpMovement;

public class JetPackMovement : MonoBehaviour
{
    public float jetPackMaxSpeed;
    public float jetPackAccelerationDuration;
    public float pauseGravityScale;
    public float fuelAmountRefill;
    public float fuelAmountRefillSpeed;

    public float fuelAmount;
    public GameObject fuelBar;

    public enum JetPackStateTransition
    {
        Performed, Pause, Stoped, Unknown
    }

    public enum JetPackStateUpdate
    {
        Performed, Pause, Stoped
    }

    private MovementStateUpdater<JetPackStateUpdate, JetPackStateTransition> movementStateUpdater =
        new MovementStateUpdater<JetPackStateUpdate, JetPackStateTransition>(
            JetPackStateUpdate.Stoped,
            updater =>
            {
                switch(updater.getCurrent())
                {
                    case (JetPackStateUpdate.Pause):
                        return JetPackStateTransition.Pause;
                    case (JetPackStateUpdate.Performed):
                        return JetPackStateTransition.Performed;
                    case (JetPackStateUpdate.Stoped):
                        return JetPackStateTransition.Stoped;
                }
                return JetPackStateTransition.Unknown;
            }
            );

    // Update is called once per frame
    public JetPackStateTransition updateState(InputActionPhase jumpButtonState, bool isGrounded, JumpStateTransition jumpState)
    {
        JetPackStateTransition currentState =  movementStateUpdater.transitionState();

        if ((currentState == JetPackStateTransition.Stoped || currentState == JetPackStateTransition.Pause)
            && !isGrounded
            && (jumpButtonState == InputActionPhase.Started || jumpButtonState == InputActionPhase.Performed)
            && jumpState == JumpStateTransition.Stoped
            && fuelAmount > 0)
        {
            movementStateUpdater.update(JetPackStateUpdate.Performed);
        }
        else if (currentState == JetPackStateTransition.Performed && jumpButtonState == InputActionPhase.Canceled)
        {
            movementStateUpdater.update(JetPackStateUpdate.Pause);
        } else if ((currentState == JetPackStateTransition.Pause || currentState == JetPackStateTransition.Performed) && isGrounded)
        {
            movementStateUpdater.update(JetPackStateUpdate.Stoped);
        }
        if(currentState == JetPackStateTransition.Performed && fuelAmount <= 0)
        {
            movementStateUpdater.update(JetPackStateUpdate.Pause);
        }

        return movementStateUpdater.apply();
    }

    // Update is called once per frame
    public void execute (Rigidbody2D rb, bool isRefillColided)
    {
        JetPackStateTransition currentState = movementStateUpdater.transitionState();

        // Manage acceleration

        switch (currentState)
        {
            case JetPackStateTransition.Performed:
                Vector2 cancelGravity = -1 * Vector2.up * rb.gravityScale * Physics2D.gravity;
                float jetPackAccelerationAmmout = getAccelerationForce(
                    rb.velocity.y,
                    jetPackMaxSpeed,
                    jetPackAccelerationDuration
                );
                rb.AddForce((Vector2.up * jetPackAccelerationAmmout + cancelGravity));
                fuelAmount--;
                break;
            case JetPackStateTransition.Pause:
                break;
            case JetPackStateTransition.Stoped:
                if(fuelAmount > fuelAmountRefill)
                {
                    fuelAmount = fuelAmountRefill;
                } else
                {
                    //fuelAmount += fuelAmountRefillSpeed;
                }
                break;
            default:
                break;
        }

        if(isRefillColided)
        {
            fuelAmount = fuelAmountRefill;
        }
        if(fuelAmount >= 0)
        {
            fuelBar.transform.localScale = new Vector3(1, 1 - (fuelAmountRefill - fuelAmount) / fuelAmountRefill);
        }
    }


    private float getAccelerationForce(float currentSpeed, float targetSpeed, float accelerationDuration)
    {
        return (targetSpeed - currentSpeed) / accelerationDuration;
    }

}
