using AE_FSM;
using DogFramework.EditorExtension;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RPGCore.AI.HFSM
{
	public class GraphBackgroundLayer : GraphLayer
	{
		public static float Small_Grid_Length { get; } = 30f;
		public static float Big_Grid_Length { get; } = 300f;
		public static Color Grid_Color { get; } = new Color(0, 0, 0, 0.2f);
		public static Color Backgraound_Color { get; } = new Color(0, 0, 0, 0.25f);
		public static int stateWidth => 480;
		public static int stateHeight => 90;
		private Vector2 mousePosition;

		public GraphBackgroundLayer(EditorWindow hFSMEditorWindow) : base(hFSMEditorWindow)
		{
		}

		public override void OnGUI(Rect rect)
		{
			base.OnGUI(rect);
			if (Event.current.type == EventType.Repaint)
			{
				EditorGUI.DrawRect(rect, Backgraound_Color);
				DrawGrid(rect, Small_Grid_Length, Grid_Color);
				DrawGrid(rect, Big_Grid_Length, Grid_Color);
			}
		}

		public override void ProcessEvent()
		{
			base.ProcessEvent();
			//拖拽
			if (Event.current.type == EventType.MouseDrag && Event.current.button == 2 && posotion.Contains(Event.current.mousePosition))
			{
				this.context.dragOffset += Event.current.delta;
				Event.current.Use();
			}

			//缩放
			if (Event.current.type == EventType.ScrollWheel && posotion.Contains(Event.current.mousePosition))
			{
				//当 f = Event.current.delta.y 为正数或零时，返回值为 1，当 f 为负数时，返回值为 -1。
				this.context.zoomFactor -= Mathf.Sign(Event.current.delta.y) / 20f;
				this.context.zoomFactor = Mathf.Clamp(this.context.zoomFactor, 0.2f, 1f);
				Event.current.Use();
			}
			//取消选中
			if (posotion.MouseOn() && !IsMouseOverAnyState(context.currentChildStatesData) && EventUtility.IsMouseDown(0))
			{
				this.context.ClearAllSelectNode();
				this.context.StopPriviewTransition();
			}
			//右键菜单
			if (posotion.IsContainsCurrentMouse() && !IsMouseOverAnyState(context.currentChildStatesData) && EventUtility.IsMouseUp(1) && this.context.HFSMController != null)
			{
				mousePosition = Event.current.mousePosition;
				CreateMenu();
			}
		}

		private void CreateMenu()
		{
			this.context.ClearAllSelectNode();

			GenericMenu genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Create State"), false, () =>
			{
				CreateState();
			});
			genericMenu.AddItem(new GUIContent("Create StateMachine"), false, () =>
			{
				CreateStateMachine();
			});
			genericMenu.ShowAsContext();
		}

		private void CreateState()
		{
			var rect = new Rect(0, 0, stateWidth, stateHeight);
			rect.center = MousePosition(mousePosition);
			context.HFSMController.CreateState(rect, context.currentStateMachine);
			context.UpdateCurrentChildStatesData();
		}

		private void CreateStateMachine()
		{
			var rect = new Rect(0, 0, stateWidth, stateHeight);
			rect.center = MousePosition(mousePosition);
			context.HFSMController.CreateStateMachine(rect, context.currentStateMachine);
			context.UpdateCurrentChildStatesData();
		}

		private void DrawGrid(Rect rect, float gridSpace, Color color)
		{
			if (rect.width < gridSpace) { return; }
			if (gridSpace == 0) { return; }
			gridSpace *= this.context.zoomFactor;
			DrawHorizontal(rect, gridSpace, color);
			DrawHorizontal(rect, -gridSpace, color, 1);
			DrawVertical(rect, gridSpace, color);
			DrawVertical(rect, -gridSpace, color, 1);
		}

		public void DrawVertical(Rect rect, float gradSpace, Color color, int startIndex = 0)
		{
			Vector2 center = rect.center + this.context.dragOffset;
			Vector2 start;
			Vector2 end;

			int i = startIndex;

			if (center.x > rect.position.x + rect.width && gradSpace < 0)
			{
				i = Mathf.CeilToInt((center.x - (rect.position.x + rect.width)) / Mathf.Abs(gradSpace));
				//Debug.Log(i);
			}

			if (center.x < rect.position.x && gradSpace > 0)
			{
				i = Mathf.CeilToInt((rect.position.x - center.x) / Mathf.Abs(gradSpace));
				//Debug.Log(i);
			}
			GUIExtension.Begin();
			do
			{
				start = new Vector2(center.x + gradSpace * i, rect.center.y - rect.height / 2);
				end = new Vector2(center.x + gradSpace * i, rect.center.y + rect.height / 2);
				if (rect.Contains((start + end) / 2))
				{
					GUIExtension.DrawLine(start, end, 5, color);
					i++;
				}
			} while (rect.Contains((start + end) / 2));
			GUIExtension.End();
		}

		public void DrawHorizontal(Rect rect, float gradSpace, Color color, int startIndex = 0)
		{
			Vector2 center = rect.center + this.context.dragOffset;
			Vector2 start;
			Vector2 end;

			int i = startIndex;

			if (center.y > rect.position.y + rect.height && gradSpace < 0)
			{
				i = Mathf.CeilToInt((center.y - (rect.position.y + rect.height)) / Mathf.Abs(gradSpace));
				//Debug.Log(i);
			}

			if (center.y < rect.position.y && gradSpace > 0)
			{
				i = Mathf.CeilToInt((rect.position.y - center.x) / Mathf.Abs(gradSpace));
				//Debug.Log(i);
			}
			GUIExtension.Begin();
			do
			{
				start = new Vector2(rect.center.x - rect.width / 2, center.y + gradSpace * i);
				end = new Vector2(rect.center.x + rect.width / 2, center.y + gradSpace * i);
				if (rect.Contains((start + end) / 2))
				{
					GUIExtension.DrawLine(start, end, 5, color);
					i++;
				}
			} while (rect.Contains((start + end) / 2));
			GUIExtension.End();
		}
	}
}