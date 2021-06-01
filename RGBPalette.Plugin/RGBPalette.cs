using BepInEx;
using HarmonyLib;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
//using UnityInjector;
//using UnityInjector.Attributes;

namespace RGBPalette.Plugin
{
	//[PluginName("RGB Palette")]
	//[PluginVersion("1.0.0.0")]
	[BepInPlugin("COM3D2.RGBPalette.Plugin", "COM3D2.RGBPalette.Plugin", "1.0.0.0")]// 버전 규칙 잇음. 반드시 2~4개의 숫자구성으로 해야함. 미준수시 못읽어들임
	[BepInProcess("COM3D2x64.exe")]
	public class RGBPalette : BaseUnityPlugin
	{
		Harmony harmony;

		private void Awake()
		{
			//UnityEngine.Object.DontDestroyOnLoad(this);
			RGBPalette.Instance = this;
		}

		public void OnEnable()
        {
			harmony=Harmony.CreateAndPatchAll(typeof(Patcher));
		}

		static Vector3 vector;//= UICamera.currentCamera.WorldToScreenPoint(this.colorPaletteManager.gameObject.transform.position);
		static Rect clientRect;// = new Rect((float)((int)vector.x + 330), (float)(-(float)((int)vector.y) + 650), 200f, 280f);

		private void OnGUI()
		{
			if (!this.visible || colorPaletteManager ==null)
			{
				return;
			}
			//vector = UICamera.currentCamera.WorldToScreenPoint(this.colorPaletteManager.gameObject.transform.position);
			//clientRect = new Rect((float)((int)vector.x + 330), (float)(-(float)((int)vector.y) + 650), 200f, 280f);
			GUI.Window(6910, clientRect, new GUI.WindowFunction(this.DrawWindow), "Color palette");
		}

		public void OnDisable()
        {
			harmony.UnpatchSelf();
		}

		private void DrawWindow(int id)
		{
			Color color = this.HSLToRGB(this.sliderHue.value, this.sliderChroma.value, this.sliderBrightness.value);
			Color col = new Color(color.r, color.g, color.b);
			//GUILayout.BeginArea(new Rect(10f, 30f, 180f, 250f));
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			this.DrawSlider("R", () => this.Round(col.r * 255f), delegate(int r)
			{
				col.r = (float)r / 255f;
			}, 0, 255);
			this.DrawSlider("G", () => this.Round(col.g * 255f), delegate(int g)
			{
				col.g = (float)g / 255f;
			}, 0, 255);
			this.DrawSlider("B", () => this.Round(col.b * 255f), delegate(int b)
			{
				col.b = (float)b / 255f;
			}, 0, 255);
			GUILayout.EndVertical();
			//GUILayout.EndArea();
			if (col != color)
			{
				this.SetColor(col);
			}
			GUI.DragWindow();
			GUI.enabled = true;
		}

		private int Round(float f)
		{
			return (int)Math.Round((double)f, MidpointRounding.AwayFromZero);
		}

		private void DrawSlider(string label, Func<int> getter, Action<int> setter, int min, int max)
		{
			int num = getter();
			int num2 = num;
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Label(label, new GUILayoutOption[]
			{
				GUILayout.Width(10f)
			});
			int.TryParse(GUILayout.TextField(num.ToString(), new GUILayoutOption[]
			{
				GUILayout.Width(30f)
			}), out num);
			num = this.Round(GUILayout.HorizontalSlider((float)num, (float)min, (float)max, new GUILayoutOption[0]));
			if (num != num2)
			{
				setter(num);
			}
			GUILayout.EndHorizontal();
		}

		private float HueToRGB(float p, float q, float t)
		{
			if (t < 0f)
			{
				t += 1f;
			}
			if (t > 1f)
			{
				t -= 1f;
			}
			if (t < 0.166666672f)
			{
				return p + (q - p) * 6f * t;
			}
			if (t < 0.5f)
			{
				return q;
			}
			if (t < 0.6666667f)
			{
				return p + (q - p) * (0.6666667f - t) * 6f;
			}
			return p;
		}

		private Color HSLToRGB(float h, float s, float l)
		{
			if (s == 0f)
			{
				return new Color(l, l, l);
			}
			float num = (l < 0.5f) ? (l * (1f + s)) : (l + s - l * s);
			float p = 2f * l - num;
			return new Color(this.HueToRGB(p, num, h + 0.333333343f), this.HueToRGB(p, num, h), this.HueToRGB(p, num, h - 0.333333343f));
		}

		private void SetColor(Color c)
		{
			float num = Math.Max(Math.Max(c.r, c.g), c.b);
			float num2 = Math.Min(Math.Min(c.r, c.g), c.b);
			float num3 = 0f;
			float num4 = (num + num2) / 2f;
			float value;
			if (num == num2)
			{
				value = (num3 = 0f);
			}
			else
			{
				float num5 = num - num2;
				value = (((double)num4 > 0.5) ? (num5 / (2f - num - num2)) : (num5 / (num + num2)));
				if (num == c.r)
				{
					num3 = (c.g - c.b) / num5 + ((c.g < c.b) ? 6f : 0f);
				}
				else if (num == c.g)
				{
					num3 = (c.b - c.r) / num5 + 2f;
				}
				else if (num == c.b)
				{
					num3 = (c.r - c.g) / num5 + 4f;
				}
				num3 /= 6f;
			}
			this.sliderHue.value = num3;
			this.sliderChroma.value = value;
			this.sliderBrightness.value = num4;
		}

		public static void OnPaletteOpen(ColorPaletteManager instance, ColorPaletteUIManager uiManager)
		{
			if (RGBPalette.Instance == null)
			{
				return;
			}
			RGBPalette.Instance.colorPaletteManager = instance;
			RGBPalette.Instance.uiManager = uiManager;
			RGBPalette.Instance.sliderHue = UTY.GetChildObject(uiManager.gameObject, "PickerPalette/HueSlider", false).GetComponent<UISlider>();
			RGBPalette.Instance.sliderChroma = UTY.GetChildObject(uiManager.gameObject, "ColorAdjustment/Saturation/SatSlider", false).GetComponent<UISlider>();
			RGBPalette.Instance.sliderBrightness = UTY.GetChildObject(uiManager.gameObject, "ColorAdjustment/Brightness/BriSlider", false).GetComponent<UISlider>();
			RGBPalette.Instance.visible = true;
			vector = UICamera.currentCamera.WorldToScreenPoint(RGBPalette.Instance.colorPaletteManager.gameObject.transform.position);
			clientRect = new Rect((float)((int)vector.x + 330), (float)(-(float)((int)vector.y) + 650), 200f, 100f);
		}

		public static void OnPaletteClose()
		{
			if (RGBPalette.Instance == null)
			{
				return;
			}
			RGBPalette.Instance.visible = false;
			RGBPalette.Instance.colorPaletteManager = null;
			RGBPalette.Instance.uiManager = null;
			RGBPalette.Instance.sliderHue = null;
			RGBPalette.Instance.sliderChroma = null;
			RGBPalette.Instance.sliderBrightness = null;
		}

		public RGBPalette()
		{
		}

		private const int WIDTH = 200;

		private const int HEIGHT = 280;

		private const int X_OFFS = 330;

		private const int Y_OFFS = 650;

		private const int M_T = 30;

		private const int M_RL = 10;

		private static RGBPalette Instance;

		private ColorPaletteManager colorPaletteManager;

		private UISlider sliderHue;

		private UISlider sliderChroma;

		private UISlider sliderBrightness;

		private ColorPaletteUIManager uiManager;

		private bool visible;
		/*
		[CompilerGenerated]
		private sealed class <>c__DisplayClass15_0
		{
			public <>c__DisplayClass15_0()
			{
			}

			internal int <DrawWindow>b__0()
			{
				return this.<>4__this.Round(this.col.r * 255f);
			}

			internal void <DrawWindow>b__1(int r)
			{
				this.col.r = (float)r / 255f;
			}

			internal int <DrawWindow>b__2()
			{
				return this.<>4__this.Round(this.col.g * 255f);
			}

			internal void <DrawWindow>b__3(int g)
			{
				this.col.g = (float)g / 255f;
			}

			internal int <DrawWindow>b__4()
			{
				return this.<>4__this.Round(this.col.b * 255f);
			}

			internal void <DrawWindow>b__5(int b)
			{
				this.col.b = (float)b / 255f;
			}

			public RGBPalette <>4__this;

			public Color col;
		}

	*/
	}
}
