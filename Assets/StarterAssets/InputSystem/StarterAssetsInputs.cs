using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool Dash;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")] //鼠标光标设置 是否锁定光标 是否用于看向
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM //如果使用新输入系统才会编译

		public void OnMove(InputValue value) //移动的时候 input system会调用这个函数，value是输入的值
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if (cursorInputForLook) //如果鼠标输入用于看向
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}


		public void OnDash(InputValue value)
		{
			Dash = value.isPressed;
		}
#endif


		public void MoveInput(Vector2 newMoveDirection) //将输入和设值分开 这样的话 不同设备输入可以调用同一个函数来设置值
		{
			move = newMoveDirection;
		}

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}

		private void DashInput(bool newDashState)
		{
			Dash = newDashState;
		}
	}

}