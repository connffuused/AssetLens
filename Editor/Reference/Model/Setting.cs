﻿using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssetLens.Reference
{
	internal sealed class Setting : ScriptableObject
	{
		public const uint INDEX_VERSION = 100;

		private static Setting instance = default;
		
		private const string k_editorCustomSettingsPath = FileSystem.SettingDirectory + "/AssetLens Setting.asset";

		[SerializeField] private bool enabled = false;
		[SerializeField] private bool pauseInPlaymode = true;
		[SerializeField] private bool traceSceneObject = false;
		[SerializeField] private bool useEditorUtilityWhenSearchDependencies = false;
		[SerializeField] private bool displayIndexerVersion = false;

		[SerializeField] private string localization = "English";
		
		internal static Setting Inst {
			get => GetOrCreateSettings();
		}

		public static bool IsEnabled {
			get => GetOrCreateSettings().enabled;
			set
			{
				GetOrCreateSettings().enabled = value;
				EditorUtility.SetDirty(GetOrCreateSettings());
			}
		}

		public static bool PauseInPlaymode => GetOrCreateSettings().pauseInPlaymode;

		public static bool TraceSceneObject => GetOrCreateSettings().traceSceneObject;

		public static bool UseEditorUtilityWhenSearchDependencies =>
			GetOrCreateSettings().useEditorUtilityWhenSearchDependencies;


		public static bool DisplayIndexerVersion => GetOrCreateSettings().displayIndexerVersion;

		internal static Localize LoadLocalization {
			get
			{
				string locale = GetOrCreateSettings().localization;
				string fullPath = Path.GetFullPath($"{FileSystem.PackageDirectory}/Languages/{locale}.json");

				string json = File.ReadAllText(fullPath);
				return JsonUtility.FromJson<Localize>(json);
			}
		}

		public static string Localization {
			set
			{
				GetOrCreateSettings().localization = value;
				EditorUtility.SetDirty(GetOrCreateSettings());
			}
		}

		private static Setting GetOrCreateSettings()
		{
			if (instance != null)
			{
				return instance;
			}

			instance = EditorGUIUtility.Load(FileSystem.SettingDirectory + "/ReferenceSetting.asset") as Setting;
			if (instance != null)
			{
				AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(instance), k_editorCustomSettingsPath);
				AssetDatabase.SaveAssets();
				
				return instance;
			}

			instance = EditorGUIUtility.Load(k_editorCustomSettingsPath) as Setting;

			if (instance == null)
			{
				instance = CreateInstance<Setting>();
				instance.enabled = false;

				if (!Directory.Exists(FileSystem.SettingDirectory))
				{
					Directory.CreateDirectory(FileSystem.SettingDirectory);
					AssetDatabase.ImportAsset(FileSystem.SettingDirectory);
				}

				AssetDatabase.CreateAsset(instance, k_editorCustomSettingsPath);
				AssetDatabase.SaveAssets();
			}

			return instance;
		}

		internal class AssetDataSettingsProviderRegister
		{
			[SettingsProvider]
			public static SettingsProvider CreateFromSettingsObject()
			{
				Object settingsObj = GetOrCreateSettings();
				
				AssetSettingsProvider provider =
					AssetSettingsProvider.CreateProviderFromObject($"Project/Asset Lens", settingsObj);

				provider.keywords =
					SettingsProvider.GetSearchKeywordsFromSerializedObject(
						new SerializedObject(settingsObj));
				return provider;
			}
		}
	}
}