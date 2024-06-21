using System;
using System.Collections.Generic;
using CommandPalette.Utils;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace CommandPalette.Basic {
    public delegate VisualElement CreateParameterFieldDelegate(CommandParameterValues parameterValues, int parameterIndex);

    public static class CommandPaletteParameterDriver {
        private static readonly Dictionary<Type, CreateParameterFieldDelegate> s_externalParameterFieldFunctions = new();

        public static void RegisterParameterFieldFunction(Type type, CreateParameterFieldDelegate createParameterFieldFunction) {
            if (!s_externalParameterFieldFunctions.ContainsKey(type)) {
                s_externalParameterFieldFunctions[type] = createParameterFieldFunction;
            }
        }

        public static bool TryRegisterParameterFieldFunction(Type type, CreateParameterFieldDelegate createParameterFieldFunction) {
            if (s_externalParameterFieldFunctions.ContainsKey(type)) {
                return false;
            }

            s_externalParameterFieldFunctions.Add(type, createParameterFieldFunction);
            return true;
        }

        public static bool IsKnownType(Type type) {
            return IsBuiltinType(type) || ExternalSupportForType(type);
        }

        public static VisualElement CreateParameterField(Type type, CommandParameterValues commandParameterValues, int index) {
            if (IsBuiltinType(type)) {
                return CreateBuiltinParameterField(type, commandParameterValues, index);
            } if (ExternalSupportForType(type)) {
                return CreateExternalParameterField(type, commandParameterValues, index);
            }

            throw new ArgumentException($"Type {type} is not supported");
        }

        private static VisualElement CreateBuiltinParameterField(Type type, CommandParameterValues commandParameterValues, int index) {
            if (type == typeof(int)) return MakeField<IntegerField, int>(commandParameterValues, index);
            if (type == typeof(float)) return MakeField<FloatField, float>(commandParameterValues, index);
            if (type == typeof(double)) return MakeField<DoubleField, double>(commandParameterValues, index);
            if (type == typeof(string)) return MakeField<TextField, string>(commandParameterValues, index);
            if (type == typeof(bool)) return MakeField<Toggle, bool>(commandParameterValues, index);
            if (type == typeof(Vector2)) return MakeField<Vector2Field, Vector2>(commandParameterValues, index);
            if (type == typeof(Vector2Int)) return MakeField<Vector2IntField, Vector2Int>(commandParameterValues, index);
            if (type == typeof(Vector3)) return MakeField<Vector3Field, Vector3>(commandParameterValues, index);
            if (type == typeof(Vector3Int)) return MakeField<Vector3IntField, Vector3Int>(commandParameterValues, index);
            if (type == typeof(Vector4)) return MakeField<Vector4Field, Vector4>(commandParameterValues, index);
            if (type == typeof(Color)) return MakeField<ColorField, Color>(commandParameterValues, index);
            if (type == typeof(Color32)) return MakeConvertibleField<ColorField, Color, Color32>(commandParameterValues, index, color => color, color32 => color32);
            if (type == typeof(Rect)) return MakeField<RectField, Rect>(commandParameterValues, index);
            if (type == typeof(RectInt)) return MakeField<RectIntField, RectInt>(commandParameterValues, index);
            if (type == typeof(Quaternion)) return MakeConvertibleField<Vector3Field, Vector3, Quaternion>(commandParameterValues, index, Quaternion.Euler, quaternion => quaternion.eulerAngles);
            if (type == typeof(AnimationCurve)) return MakeField<CurveField, AnimationCurve>(commandParameterValues, index);
            if (type == typeof(Gradient)) return MakeField<GradientField, Gradient>(commandParameterValues, index);
            if (type == typeof(Bounds)) return MakeField<BoundsField, Bounds>(commandParameterValues, index);
            if (type == typeof(BoundsInt)) return MakeField<BoundsIntField, BoundsInt>(commandParameterValues, index);
            if (type == typeof(LayerMask)) return MakeLayerMaskField(commandParameterValues, index);
            if (type.InheritsFrom<Object>()) return MakeObjectField(commandParameterValues, index);
            if (type.InheritsFrom<Enum>()) return MakeField<EnumField, Enum>(commandParameterValues, index);

            throw new NotImplementedException($"Type {type} is not supported");
        }

        public static VisualElement MakeField<TField, TValueType>(CommandParameterValues commandParameterValues, int index) where TField : BaseField<TValueType>, new() {
            TField field = new() {
                label = $"{commandParameterValues.Parameters[index].DisplayName}",
                value = (TValueType)commandParameterValues.Values[index],
                userData = index,
            };

            field.AddToClassList("parameter-field");
            field.RegisterValueChangedCallback(evt => {
                commandParameterValues.Values[index] = evt.newValue;
            });

            if (!string.IsNullOrWhiteSpace(commandParameterValues.Parameters[index].Description)) {
                field.hierarchy[0].Add(new Label(commandParameterValues.Parameters[index].Description)
                                       .WithClasses("parameter-field-description"));
                field.hierarchy[0].tooltip = commandParameterValues.Parameters[index].Description;
            }

            return field;
        }

        public static VisualElement MakeConvertibleField<TField, TFieldValueType, TTargetValueType> (
            CommandParameterValues commandParameterValues, int index,
            Func<TFieldValueType, TTargetValueType> a, Func<TTargetValueType, TFieldValueType> b
        ) where TField : BaseField<TFieldValueType>, new() {
            TField field = new() {
                label = $"{commandParameterValues.Parameters[index].DisplayName}",
                value = b((TTargetValueType)commandParameterValues.Values[index]),
                userData = index,
            };

            field.AddToClassList("parameter-field");
            field.RegisterValueChangedCallback(evt => {
                commandParameterValues.Values[index] = a(evt.newValue);
            });

            if (!string.IsNullOrWhiteSpace(commandParameterValues.Parameters[index].Description)) {
                field.hierarchy[0].Add(new Label(commandParameterValues.Parameters[index].Description)
                                       .WithClasses("parameter-field-description"));
                field.hierarchy[0].tooltip = commandParameterValues.Parameters[index].Description;
            }

            return field;
        }

        private static VisualElement MakeLayerMaskField(CommandParameterValues commandParameterValues, int index) {
            LayerMaskField field = new() {
                label = $"{commandParameterValues.Parameters[index].DisplayName}",
                value = (LayerMask)commandParameterValues.Values[index],
                userData = index,
            };

            field.AddToClassList("parameter-field");
            field.RegisterValueChangedCallback(evt => {
                commandParameterValues.Values[index] = evt.newValue;
            });

            if (!string.IsNullOrWhiteSpace(commandParameterValues.Parameters[index].Description)) {
                field.hierarchy[0].Add(new Label(commandParameterValues.Parameters[index].Description)
                                       .WithClasses("parameter-field-description"));
                field.hierarchy[0].tooltip = commandParameterValues.Parameters[index].Description;
            }

            return field;
        }

        private static VisualElement MakeObjectField(CommandParameterValues commandParameterValues, int index) {
            ObjectField field = new() {
                label = $"{commandParameterValues.Parameters[index].DisplayName}",
                value = (Object)commandParameterValues.Values[index],
                objectType = commandParameterValues.Parameters[index].Type,
                userData = index,
            };

            field.AddToClassList("parameter-field");
            field.RegisterValueChangedCallback(evt => {
                commandParameterValues.Values[index] = evt.newValue;
            });

            if (!string.IsNullOrWhiteSpace(commandParameterValues.Parameters[index].Description)) {
                field.hierarchy[0].Add(new Label(commandParameterValues.Parameters[index].Description)
                                       .WithClasses("parameter-field-description"));
                field.hierarchy[0].tooltip = commandParameterValues.Parameters[index].Description;
            }

            return field;
        }

        private static VisualElement CreateExternalParameterField(Type type, CommandParameterValues commandParameterValues, int index) {
            if (!s_externalParameterFieldFunctions.TryGetValue(type, out var createParameterFieldDelegate) || createParameterFieldDelegate == null) {
                throw new NotImplementedException($"Type {type} is not supported");
            }

            return createParameterFieldDelegate(commandParameterValues, index);
        }

        private static bool IsBuiltinType(Type type) {
            return type == typeof(int) || type == typeof(float) || type == typeof(double) || type == typeof(string) || type == typeof(bool) || type == typeof(Vector2)
                || type == typeof(Vector2Int) || type == typeof(Vector3) || type == typeof(Vector3Int) || type == typeof(Vector4) || type == typeof(Color)
                || type == typeof(Color32) || type == typeof(Rect) || type == typeof(RectInt) || type == typeof(Bounds) || type == typeof(BoundsInt) || type == typeof(Quaternion)
                || type == typeof(AnimationCurve) || type == typeof(Gradient) || type == typeof(LayerMask)
                || type.InheritsFrom<Object>() || type.InheritsFrom<Enum>();
        }

        private static bool ExternalSupportForType(Type type) {
            return s_externalParameterFieldFunctions.ContainsKey(type);
        }
    }
}