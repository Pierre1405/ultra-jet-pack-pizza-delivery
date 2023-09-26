using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.scripts.physics
{
    public class JumpMovement : MonoBehaviour
    {
        public float jumpSpeed;
        public float firstImpulseMultiplier;
        public float maxHeight;
        public float coyoteDuration;
        public float jumpCutMultiplier;


        public enum JumpStateTransition
        {
            JustStarted, Performed, JustFinished, Stoped, Unknown
        }

        public enum JumpStateUpdate
        {
            Performed, Stoped
        }

        private float coyoteGroundedTimer = 0;
        private float coyoteJumpTimer = 0;
        private float initialHeight = 0;
        private MovementStateUpdater<JumpStateUpdate, JumpStateTransition> jumpStateUpdater =
            new(JumpStateUpdate.Stoped, updater => {
                switch (updater.getPrevious(), updater.getCurrent())
                {
                    case (JumpStateUpdate.Stoped, JumpStateUpdate.Performed):
                        return JumpStateTransition.JustStarted;
                    case (JumpStateUpdate.Performed, JumpStateUpdate.Performed):
                        return JumpStateTransition.Performed;
                    case (JumpStateUpdate.Performed, JumpStateUpdate.Stoped):
                        return JumpStateTransition.JustFinished;
                    case (JumpStateUpdate.Stoped, JumpStateUpdate.Stoped):
                        return JumpStateTransition.Stoped;
                }
                return JumpStateTransition.Unknown;
            });

        public JumpStateTransition updateState(InputActionPhase jumpButtonState, bool isGrounded, Rigidbody2D rb)
        {
            #region CoyoteTimer
            if (isGrounded)
            {
                coyoteGroundedTimer = coyoteDuration;
            }

            if (jumpButtonState == InputActionPhase.Canceled)
            {
                coyoteJumpTimer = coyoteDuration;
            }
            coyoteGroundedTimer -= Time.fixedDeltaTime;
            coyoteJumpTimer -= Time.fixedDeltaTime;
            #endregion CoyoteTimer

            JumpStateTransition jumpState = jumpStateUpdater.transitionState();
            #region StopJump
            if ((jumpState == JumpStateTransition.JustStarted || jumpState == JumpStateTransition.Performed)
                && jumpButtonState == InputActionPhase.Canceled ||
                (rb.position.y - initialHeight) >= maxHeight ||
                (!(jumpState == JumpStateTransition.JustStarted) && rb.velocity.y == 0))
            {
                jumpStateUpdater.update(JumpStateUpdate.Stoped);
            }
            #endregion StopJump

            #region Jump
            if (jumpState == JumpStateTransition.JustStarted && jumpButtonState == InputActionPhase.Performed || jumpButtonState == InputActionPhase.Started)
            {
                jumpStateUpdater.update(JumpStateUpdate.Performed);
            }
            #endregion Jump

            #region StartJump
            bool canLaunchJump = coyoteGroundedTimer > 0 && coyoteJumpTimer > 0 && (jumpState == JumpStateTransition.JustFinished || jumpState == JumpStateTransition.Stoped);
            if (canLaunchJump &&
                (jumpButtonState == InputActionPhase.Performed || jumpButtonState == InputActionPhase.Started))
            {
                jumpStateUpdater.update(JumpStateUpdate.Performed);
            }
            #endregion StartJump

            return jumpStateUpdater.apply();
        }

        public void execute(Rigidbody2D rb)
        {

            float jumpForceAmount = jumpSpeed - rb.velocity.y + rb.gravityScale;
            Vector2 jumpForce = Vector2.up * jumpForceAmount;
            switch (jumpStateUpdater.transitionState())
            {
                case JumpStateTransition.JustStarted:
                    rb.AddForce(jumpForce * firstImpulseMultiplier, ForceMode2D.Impulse);
                    initialHeight = rb.position.y;
                    break;
                case JumpStateTransition.Performed:
                    rb.AddForce(jumpForce, ForceMode2D.Impulse);
                    break;
                case JumpStateTransition.JustFinished:
                    float jumpCut = rb.velocity.y * jumpCutMultiplier;
                    rb.AddForce(Vector2.down * jumpCutMultiplier, ForceMode2D.Impulse);
                    break;
                case JumpStateTransition.Stoped:
                    break;
                default:
                    break;
            }
        }
    }
}