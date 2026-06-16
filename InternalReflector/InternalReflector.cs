using System;
using System.Reflection;

namespace InternalReflector {
	/// <summary>
	/// Non-generic entry point for reflecting against a runtime <see cref="Type"/>.
	/// Useful for static classes, which cannot be used as generic type arguments.
	/// </summary>
	public static class InternalReflector {
		/// <summary>
		/// Creates a reflector bound to the specified type.
		/// </summary>
		/// <param name="targetType">The target type to reflect upon</param>
		/// <returns>A type-bound reflector</returns>
		public static TypeReflector For(Type targetType) {
			if (targetType == null)
				throw new ArgumentNullException(nameof(targetType));

			return new TypeReflector(targetType);
		}
	}

	/// <summary>
	/// A type-bound reflector that provides access to internal or private fields, properties, and methods.
	/// </summary>
	public sealed class TypeReflector {
		private readonly Type _targetType;

		internal TypeReflector(Type targetType) {
			_targetType = targetType;
		}

#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		public object Call(string methodName, params object[] parameters) {
#else
		public object? Call(string methodName, params object[] parameters) {
#endif
			if (string.IsNullOrEmpty(methodName))
				throw new ArgumentException("Method name cannot be null or empty", nameof(methodName));

			var method = FindStaticMethod(methodName, parameters);
			if (method == null)
				throw new InvalidOperationException($"Method '{methodName}' not found on type '{_targetType.Name}'");

			return method.Invoke(null, parameters);
		}

#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		public object Call(object instance, string methodName, params object[] parameters) {
#else
		public object? Call(object instance, string methodName, params object[] parameters) {
#endif
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (string.IsNullOrEmpty(methodName))
				throw new ArgumentException("Method name cannot be null or empty", nameof(methodName));
			if (!_targetType.IsAssignableFrom(instance.GetType()))
				throw new ArgumentException($"Instance must be assignable to type '{_targetType.FullName}'", nameof(instance));

			var method = FindInstanceMethod(methodName, parameters);
			if (method == null)
				throw new InvalidOperationException($"Method '{methodName}' not found on type '{_targetType.Name}'");

			return method.Invoke(instance, parameters);
		}

#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		public TField GetField<TField>(string fieldName) {
#else
		public TField? GetField<TField>(string fieldName) {
#endif
			if (string.IsNullOrEmpty(fieldName))
				throw new ArgumentException("Field name cannot be null or empty", nameof(fieldName));

			var field = FindStaticField(fieldName);
			if (field == null)
				throw new InvalidOperationException($"Field '{fieldName}' not found on type '{_targetType.Name}'");

			var value = field.GetValue(null);
			return value is null ? default(TField) : (TField)value;
		}

#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		public TField GetField<TField>(object instance, string fieldName) {
#else
		public TField? GetField<TField>(object instance, string fieldName) {
#endif
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (!_targetType.IsAssignableFrom(instance.GetType()))
				throw new ArgumentException($"Instance must be assignable to type '{_targetType.FullName}'", nameof(instance));
			if (string.IsNullOrEmpty(fieldName))
				throw new ArgumentException("Field name cannot be null or empty", nameof(fieldName));

			var field = FindInstanceField(fieldName);
			if (field == null)
				throw new InvalidOperationException($"Field '{fieldName}' not found on type '{_targetType.Name}'");

			var value = field.GetValue(instance);
			return value is null ? default(TField) : (TField)value;
		}

		public void SetField<TField>(string fieldName, TField value) {
			if (string.IsNullOrEmpty(fieldName))
				throw new ArgumentException("Field name cannot be null or empty", nameof(fieldName));

			var field = FindStaticField(fieldName);
			if (field == null)
				throw new InvalidOperationException($"Field '{fieldName}' not found on type '{_targetType.Name}'");

			field.SetValue(null, value);
		}

		public void SetField<TField>(object instance, string fieldName, TField value) {
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (!_targetType.IsAssignableFrom(instance.GetType()))
				throw new ArgumentException($"Instance must be assignable to type '{_targetType.FullName}'", nameof(instance));
			if (string.IsNullOrEmpty(fieldName))
				throw new ArgumentException("Field name cannot be null or empty", nameof(fieldName));

			var field = FindInstanceField(fieldName);
			if (field == null)
				throw new InvalidOperationException($"Field '{fieldName}' not found on type '{_targetType.Name}'");

			field.SetValue(instance, value);
		}

#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		public TProperty GetProperty<TProperty>(string propertyName) {
#else
		public TProperty? GetProperty<TProperty>(string propertyName) {
#endif
			if (string.IsNullOrEmpty(propertyName))
				throw new ArgumentException("Property name cannot be null or empty", nameof(propertyName));

			var property = FindStaticProperty(propertyName);
			if (property == null)
				throw new InvalidOperationException($"Property '{propertyName}' not found on type '{_targetType.Name}'");

			var value = property.GetValue(null, null);
			return value is null ? default(TProperty) : (TProperty)value;
		}

#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		public TProperty GetProperty<TProperty>(object instance, string propertyName) {
#else
		public TProperty? GetProperty<TProperty>(object instance, string propertyName) {
#endif
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (!_targetType.IsAssignableFrom(instance.GetType()))
				throw new ArgumentException($"Instance must be assignable to type '{_targetType.FullName}'", nameof(instance));
			if (string.IsNullOrEmpty(propertyName))
				throw new ArgumentException("Property name cannot be null or empty", nameof(propertyName));

			var property = FindInstanceProperty(propertyName);
			if (property == null)
				throw new InvalidOperationException($"Property '{propertyName}' not found on type '{_targetType.Name}'");

			var value = property.GetValue(instance, null);
			return value is null ? default(TProperty) : (TProperty)value;
		}

		public void SetProperty<TProperty>(string propertyName, TProperty value) {
			if (string.IsNullOrEmpty(propertyName))
				throw new ArgumentException("Property name cannot be null or empty", nameof(propertyName));

			var property = FindStaticProperty(propertyName);
			if (property == null)
				throw new InvalidOperationException($"Property '{propertyName}' not found on type '{_targetType.Name}'");

			property.SetValue(null, value, null);
		}

		public void SetProperty<TProperty>(object instance, string propertyName, TProperty value) {
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (!_targetType.IsAssignableFrom(instance.GetType()))
				throw new ArgumentException($"Instance must be assignable to type '{_targetType.FullName}'", nameof(instance));
			if (string.IsNullOrEmpty(propertyName))
				throw new ArgumentException("Property name cannot be null or empty", nameof(propertyName));

			var property = FindInstanceProperty(propertyName);
			if (property == null)
				throw new InvalidOperationException($"Property '{propertyName}' not found on type '{_targetType.Name}'");

			property.SetValue(instance, value, null);
		}

		private Type[] GetParameterTypes(object[] parameters) {
			var parameterTypes = new Type[parameters.Length];
			for (int i = 0; i < parameters.Length; i++) {
				parameterTypes[i] = parameters[i]?.GetType() ?? typeof(object);
			}
			return parameterTypes;
		}

		private MethodInfo[] GetMethods(BindingFlags scopeFlags) {
			return _targetType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | scopeFlags);
		}

#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		private MethodInfo FindStaticMethod(string methodName, object[] parameters) {
#else
		private MethodInfo? FindStaticMethod(string methodName, object[] parameters) {
#endif
			return FindMethodInScope(methodName, parameters, BindingFlags.Static);
		}

#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		private MethodInfo FindInstanceMethod(string methodName, object[] parameters) {
#else
		private MethodInfo? FindInstanceMethod(string methodName, object[] parameters) {
#endif
			return FindMethodInScope(methodName, parameters, BindingFlags.Instance);
		}

#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		private MethodInfo FindMethodInScope(string methodName, object[] parameters, BindingFlags scopeFlags) {
#else
		private MethodInfo? FindMethodInScope(string methodName, object[] parameters, BindingFlags scopeFlags) {
#endif
			var parameterTypes = GetParameterTypes(parameters);
			var method = _targetType.GetMethod(methodName,
				BindingFlags.NonPublic | BindingFlags.Public | scopeFlags,
				null, parameterTypes, null);

			if (method != null)
				return method;

			var methods = GetMethods(scopeFlags);
			foreach (var m in methods) {
				if (m.Name == methodName && m.GetParameters().Length == parameters.Length) {
					return m;
				}
			}

			return null;
		}

#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		private FieldInfo FindStaticField(string fieldName) {
#else
		private FieldInfo? FindStaticField(string fieldName) {
#endif
			return _targetType.GetField(fieldName,
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
		}

#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		private FieldInfo FindInstanceField(string fieldName) {
#else
		private FieldInfo? FindInstanceField(string fieldName) {
#endif
			return _targetType.GetField(fieldName,
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		}

#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		private PropertyInfo FindStaticProperty(string propertyName) {
#else
		private PropertyInfo? FindStaticProperty(string propertyName) {
#endif
			return _targetType.GetProperty(propertyName,
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
		}

#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		private PropertyInfo FindInstanceProperty(string propertyName) {
#else
		private PropertyInfo? FindInstanceProperty(string propertyName) {
#endif
			return _targetType.GetProperty(propertyName,
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		}
	}

	/// <summary>
	/// A wrapper around .NET Reflection that provides access to internal or private fields, properties, and methods in classes.
	/// </summary>
	/// <typeparam name="TType">The type to reflect upon</typeparam>
	public static class InternalReflector<TType> {
		private static readonly Type TargetType = typeof(TType);

		/// <summary>
		/// Calls a private or internal method on the target type.
		/// </summary>
		/// <param name="methodName">The name of the method to call</param>
		/// <param name="parameters">The parameters to pass to the method</param>
		/// <returns>The return value of the method, or null if the method returns void</returns>
#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		public static object Call(string methodName, params object[] parameters) {
#else
		public static object? Call(string methodName, params object[] parameters) {
#endif
			if (string.IsNullOrEmpty(methodName))
				throw new ArgumentException("Method name cannot be null or empty", nameof(methodName));

			var method = FindStaticMethod(methodName, parameters);
			if (method == null)
				throw new InvalidOperationException($"Method '{methodName}' not found on type '{TargetType.Name}'");

			return method.Invoke(null, parameters);
		}

		/// <summary>
		/// Calls a private or internal instance method on the specified instance.
		/// </summary>
		/// <param name="instance">The instance to call the method on</param>
		/// <param name="methodName">The name of the method to call</param>
		/// <param name="parameters">The parameters to pass to the method</param>
		/// <returns>The return value of the method, or null if the method returns void</returns>
#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		public static object Call(TType instance, string methodName, params object[] parameters) {
#else
		public static object? Call(TType instance, string methodName, params object[] parameters) {
#endif
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (string.IsNullOrEmpty(methodName))
				throw new ArgumentException("Method name cannot be null or empty", nameof(methodName));

			var method = FindInstanceMethod(methodName, parameters);
			if (method == null)
				throw new InvalidOperationException($"Method '{methodName}' not found on type '{TargetType.Name}'");

			return method.Invoke(instance, parameters);
		}

		/// <summary>
		/// Calls a private or internal instance method on the specified instance object.
		/// Useful when the compile-time type of the instance is not exactly <typeparamref name="TType"/>.
		/// </summary>
		/// <param name="instance">The instance object to call the method on</param>
		/// <param name="methodName">The name of the method to call</param>
		/// <param name="parameters">The parameters to pass to the method</param>
		/// <returns>The return value of the method, or null if the method returns void</returns>
#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		public static object Call(object instance, string methodName, params object[] parameters) {
#else
		public static object? Call(object instance, string methodName, params object[] parameters) {
#endif
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (string.IsNullOrEmpty(methodName))
				throw new ArgumentException("Method name cannot be null or empty", nameof(methodName));
			if (!(instance is TType typedInstance))
				throw new ArgumentException($"Instance must be assignable to type '{TargetType.FullName}'", nameof(instance));

			return Call(typedInstance, methodName, parameters);
		}

		/// <summary>
		/// Gets the value of a private or internal field.
		/// </summary>
		/// <typeparam name="TField">The type of the field</typeparam>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>The value of the field</returns>
#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		public static TField GetField<TField>(string fieldName) {
#else
		public static TField? GetField<TField>(string fieldName) {
#endif
			if (string.IsNullOrEmpty(fieldName))
				throw new ArgumentException("Field name cannot be null or empty", nameof(fieldName));

			var field = FindStaticField(fieldName);
			if (field == null)
				throw new InvalidOperationException($"Field '{fieldName}' not found on type '{TargetType.Name}'");

			var value = field.GetValue(null);
			return value is null ? default(TField) : (TField)value;
		}

		/// <summary>
		/// Gets the value of a private or internal instance field.
		/// </summary>
		/// <typeparam name="TField">The type of the field</typeparam>
		/// <param name="instance">The instance to get the field from</param>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>The value of the field</returns>
#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		public static TField GetField<TField>(TType instance, string fieldName) {
#else
		public static TField? GetField<TField>(TType instance, string fieldName) {
#endif
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (string.IsNullOrEmpty(fieldName))
				throw new ArgumentException("Field name cannot be null or empty", nameof(fieldName));

			var field = FindInstanceField(fieldName);
			if (field == null)
				throw new InvalidOperationException($"Field '{fieldName}' not found on type '{TargetType.Name}'");

			var value = field.GetValue(instance);
			return value is null ? default(TField) : (TField)value;
		}

		/// <summary>
		/// Gets the value of a private or internal instance field from the specified instance object.
		/// Useful when the compile-time type of the instance is not exactly <typeparamref name="TType"/>.
		/// </summary>
		/// <typeparam name="TField">The type of the field</typeparam>
		/// <param name="instance">The instance object to get the field from</param>
		/// <param name="fieldName">The name of the field</param>
		/// <returns>The value of the field</returns>
#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		public static TField GetField<TField>(object instance, string fieldName) {
#else
		public static TField? GetField<TField>(object instance, string fieldName) {
#endif
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (!(instance is TType typedInstance))
				throw new ArgumentException($"Instance must be assignable to type '{TargetType.FullName}'", nameof(instance));

			return GetField<TField>(typedInstance, fieldName);
		}

		/// <summary>
		/// Sets the value of a private or internal field.
		/// </summary>
		/// <typeparam name="TField">The type of the field</typeparam>
		/// <param name="fieldName">The name of the field</param>
		/// <param name="value">The value to set</param>
		public static void SetField<TField>(string fieldName, TField value) {
			if (string.IsNullOrEmpty(fieldName))
				throw new ArgumentException("Field name cannot be null or empty", nameof(fieldName));

			var field = FindStaticField(fieldName);
			if (field == null)
				throw new InvalidOperationException($"Field '{fieldName}' not found on type '{TargetType.Name}'");

			field.SetValue(null, value);
		}

		/// <summary>
		/// Sets the value of a private or internal instance field.
		/// </summary>
		/// <typeparam name="TField">The type of the field</typeparam>
		/// <param name="instance">The instance to set the field on</param>
		/// <param name="fieldName">The name of the field</param>
		/// <param name="value">The value to set</param>
		public static void SetField<TField>(TType instance, string fieldName, TField value) {
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (string.IsNullOrEmpty(fieldName))
				throw new ArgumentException("Field name cannot be null or empty", nameof(fieldName));

			var field = FindInstanceField(fieldName);
			if (field == null)
				throw new InvalidOperationException($"Field '{fieldName}' not found on type '{TargetType.Name}'");

			field.SetValue(instance, value);
		}

		/// <summary>
		/// Sets the value of a private or internal instance field on the specified instance object.
		/// Useful when the compile-time type of the instance is not exactly <typeparamref name="TType"/>.
		/// </summary>
		/// <typeparam name="TField">The type of the field</typeparam>
		/// <param name="instance">The instance object to set the field on</param>
		/// <param name="fieldName">The name of the field</param>
		/// <param name="value">The value to set</param>
		public static void SetField<TField>(object instance, string fieldName, TField value) {
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (!(instance is TType typedInstance))
				throw new ArgumentException($"Instance must be assignable to type '{TargetType.FullName}'", nameof(instance));

			SetField(typedInstance, fieldName, value);
		}

		/// <summary>
		/// Gets the value of a private or internal property.
		/// </summary>
		/// <typeparam name="TProperty">The type of the property</typeparam>
		/// <param name="propertyName">The name of the property</param>
		/// <returns>The value of the property</returns>
#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		public static TProperty GetProperty<TProperty>(string propertyName) {
#else
		public static TProperty? GetProperty<TProperty>(string propertyName) {
#endif
			if (string.IsNullOrEmpty(propertyName))
				throw new ArgumentException("Property name cannot be null or empty", nameof(propertyName));

			var property = FindStaticProperty(propertyName);
			if (property == null)
				throw new InvalidOperationException($"Property '{propertyName}' not found on type '{TargetType.Name}'");

			var value = property.GetValue(null, null);
			return value is null ? default(TProperty) : (TProperty)value;
		}

		/// <summary>
		/// Gets the value of a private or internal instance property.
		/// </summary>
		/// <typeparam name="TProperty">The type of the property</typeparam>
		/// <param name="instance">The instance to get the property from</param>
		/// <param name="propertyName">The name of the property</param>
		/// <returns>The value of the property</returns>
#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		public static TProperty GetProperty<TProperty>(TType instance, string propertyName) {
#else
		public static TProperty? GetProperty<TProperty>(TType instance, string propertyName) {
#endif
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (string.IsNullOrEmpty(propertyName))
				throw new ArgumentException("Property name cannot be null or empty", nameof(propertyName));

			var property = FindInstanceProperty(propertyName);
			if (property == null)
				throw new InvalidOperationException($"Property '{propertyName}' not found on type '{TargetType.Name}'");

			var value = property.GetValue(instance, null);
			return value is null ? default(TProperty) : (TProperty)value;
		}

		/// <summary>
		/// Gets the value of a private or internal instance property from the specified instance object.
		/// Useful when the compile-time type of the instance is not exactly <typeparamref name="TType"/>.
		/// </summary>
		/// <typeparam name="TProperty">The type of the property</typeparam>
		/// <param name="instance">The instance object to get the property from</param>
		/// <param name="propertyName">The name of the property</param>
		/// <returns>The value of the property</returns>
#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		public static TProperty GetProperty<TProperty>(object instance, string propertyName) {
#else
		public static TProperty? GetProperty<TProperty>(object instance, string propertyName) {
#endif
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (!(instance is TType typedInstance))
				throw new ArgumentException($"Instance must be assignable to type '{TargetType.FullName}'", nameof(instance));

			return GetProperty<TProperty>(typedInstance, propertyName);
		}

		/// <summary>
		/// Sets the value of a private or internal property.
		/// </summary>
		/// <typeparam name="TProperty">The type of the property</typeparam>
		/// <param name="propertyName">The name of the property</param>
		/// <param name="value">The value to set</param>
		public static void SetProperty<TProperty>(string propertyName, TProperty value) {
			if (string.IsNullOrEmpty(propertyName))
				throw new ArgumentException("Property name cannot be null or empty", nameof(propertyName));

			var property = FindStaticProperty(propertyName);
			if (property == null)
				throw new InvalidOperationException($"Property '{propertyName}' not found on type '{TargetType.Name}'");

			property.SetValue(null, value, null);
		}

		/// <summary>
		/// Sets the value of a private or internal instance property.
		/// </summary>
		/// <typeparam name="TProperty">The type of the property</typeparam>
		/// <param name="instance">The instance to set the property on</param>
		/// <param name="propertyName">The name of the property</param>
		/// <param name="value">The value to set</param>
		public static void SetProperty<TProperty>(TType instance, string propertyName, TProperty value) {
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (string.IsNullOrEmpty(propertyName))
				throw new ArgumentException("Property name cannot be null or empty", nameof(propertyName));

			var property = FindInstanceProperty(propertyName);
			if (property == null)
				throw new InvalidOperationException($"Property '{propertyName}' not found on type '{TargetType.Name}'");

			property.SetValue(instance, value, null);
		}

		/// <summary>
		/// Sets the value of a private or internal instance property on the specified instance object.
		/// Useful when the compile-time type of the instance is not exactly <typeparamref name="TType"/>.
		/// </summary>
		/// <typeparam name="TProperty">The type of the property</typeparam>
		/// <param name="instance">The instance object to set the property on</param>
		/// <param name="propertyName">The name of the property</param>
		/// <param name="value">The value to set</param>
		public static void SetProperty<TProperty>(object instance, string propertyName, TProperty value) {
			if (instance == null)
				throw new ArgumentNullException(nameof(instance));
			if (!(instance is TType typedInstance))
				throw new ArgumentException($"Instance must be assignable to type '{TargetType.FullName}'", nameof(instance));

			SetProperty(typedInstance, propertyName, value);
		}

		private static Type[] GetParameterTypes(object[] parameters) {
			var parameterTypes = new Type[parameters.Length];
			for (int i = 0; i < parameters.Length; i++) {
				parameterTypes[i] = parameters[i]?.GetType() ?? typeof(object);
			}
			return parameterTypes;
		}

		private static MethodInfo[] GetMethods(BindingFlags scopeFlags) {
			return TargetType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | scopeFlags);
		}

#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		private static MethodInfo FindStaticMethod(string methodName, object[] parameters) {
#else
		private static MethodInfo? FindStaticMethod(string methodName, object[] parameters) {
#endif
			return FindMethodInScope(methodName, parameters, BindingFlags.Static);
		}

#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		private static MethodInfo FindInstanceMethod(string methodName, object[] parameters) {
#else
		private static MethodInfo? FindInstanceMethod(string methodName, object[] parameters) {
#endif
			return FindMethodInScope(methodName, parameters, BindingFlags.Instance);
		}

#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		private static MethodInfo FindMethodInScope(string methodName, object[] parameters, BindingFlags scopeFlags) {
#else
		private static MethodInfo? FindMethodInScope(string methodName, object[] parameters, BindingFlags scopeFlags) {
#endif
			var parameterTypes = GetParameterTypes(parameters);
			var method = TargetType.GetMethod(methodName,
				BindingFlags.NonPublic | BindingFlags.Public | scopeFlags,
				null, parameterTypes, null);

			if (method != null)
				return method;

			var methods = GetMethods(scopeFlags);
			foreach (var m in methods) {
				if (m.Name == methodName && m.GetParameters().Length == parameters.Length) {
					return m;
				}
			}

			return null;
		}

#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		private static FieldInfo FindStaticField(string fieldName) {
#else
		private static FieldInfo? FindStaticField(string fieldName) {
#endif
			return TargetType.GetField(fieldName,
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
		}

#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		private static FieldInfo FindInstanceField(string fieldName) {
#else
		private static FieldInfo? FindInstanceField(string fieldName) {
#endif
			return TargetType.GetField(fieldName,
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		}

#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		private static PropertyInfo FindStaticProperty(string propertyName) {
#else
		private static PropertyInfo? FindStaticProperty(string propertyName) {
#endif
			return TargetType.GetProperty(propertyName,
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
		}

#if NET20 || NET35 || NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48 || NET481
		private static PropertyInfo FindInstanceProperty(string propertyName) {
#else
		private static PropertyInfo? FindInstanceProperty(string propertyName) {
#endif
			return TargetType.GetProperty(propertyName,
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		}
	}
}
