// (C) 2014 ERAL
// Distributed under the Boost Software License, Version 1.0.
// (See copy at http://www.boost.org/LICENSE_1_0.txt)

namespace AssetBundleShosha.Utility {
	using UnityEngine;

#if UNITY_EDITOR
	using UnityEditor;
	using System.Linq;

	[CustomPropertyDrawer(typeof(RestrictInterfaceAttribute))]
	public class RestrictInterfacePropertyDrawer : PropertyDrawer {
		#region Public methods

		/// <summary>
		/// 描画
		/// </summary>
		/// <param name="position">位置</param>
		/// <param name="property">プロパティ</param>
		/// <param name="label">ラベル</param>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var restrictInterfaceAttribute = (RestrictInterfaceAttribute)attribute;

			if (SerializedPropertyType.ObjectReference == property.propertyType) {
				using (new EditorGUI.PropertyScope(position, label, property)) {
					var propertyHeight = base.GetPropertyHeight(property, label);
					position.height = propertyHeight;

					EditorGUI.BeginChangeCheck();
					var value = EditorGUI.ObjectField(position, label, property.objectReferenceValue, fieldInfo.FieldType, true);
					if (EditorGUI.EndChangeCheck()) {
						if (null == value) {
							property.objectReferenceValue = value;
						} else if (restrictInterfaceAttribute.RestrictType.IsAssignableFrom(value.GetType())) {
							property.objectReferenceValue = value;
						} else {
							var components = ((Component)value).GetComponents<Component>().Where(x=>0 == (HideFlags.HideInInspector & x.hideFlags))
																						.Where(x=>restrictInterfaceAttribute.RestrictType.IsAssignableFrom(x.GetType()))
																						.ToArray();
							if (0 == components.Length) {
								property.objectReferenceValue = null;
							} else {
								property.objectReferenceValue = components[0];
							}
						}
					}
				}
			} else {
				EditorGUI.LabelField(position, label, new GUIContent("This type has not supported."));
			}
		}

		#endregion
	}
#endif

	public class RestrictInterfaceAttribute : PropertyAttribute {
		#region Public fields and properties

		/// <summary>
		/// 拘束型
		/// </summary>
		public System.Type RestrictType {get; set;}

		#endregion
		#region Public methods

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="restrictType">拘束型</param>
		public RestrictInterfaceAttribute(System.Type restrictType) {
			RestrictType = restrictType;
		}

		#endregion
	}
}
