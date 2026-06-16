using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Xunit;
using InternalReflector;

namespace InternalReflector.Tests {
	public class InternalReflectorTests {
		public class DateTimeFieldTests {
			[Fact]
			public void DateTimeInternalDateDataFieldIsAccessible() {
				var dateTime = new DateTime(2024, 1, 15, 10, 30, 45);
				if (TryGetDateTimeFieldAsLong(dateTime, out long dateDataLong)) {
					Assert.Equal(dateTime.Ticks, dateDataLong & 0x3FFFFFFFFFFFFFFF);
					return;
				}
				if (TryGetDateTimeFieldAsULong(dateTime, out ulong dateDataULong)) {
					Assert.Equal((ulong)dateTime.Ticks, dateDataULong & 0x3FFFFFFFFFFFFFFFUL);
					return;
				}
				throw new Xunit.Sdk.XunitException("No known internal DateTime field found for this .NET version.");
			}

			[Fact]
			public void DateTimeInternalDateDataMatchesPublicProperties() {
				var dateTime = new DateTime(2024, 1, 15);
				if (TryGetDateTimeFieldAsLong(dateTime, out long dateDataLong)) {
					Assert.Equal(dateTime.Ticks, dateDataLong & 0x3FFFFFFFFFFFFFFF);
					return;
				}
				if (TryGetDateTimeFieldAsULong(dateTime, out ulong dateDataULong)) {
					Assert.Equal((ulong)dateTime.Ticks, dateDataULong & 0x3FFFFFFFFFFFFFFFUL);
					return;
				}
				throw new Xunit.Sdk.XunitException("No known internal DateTime field found for this .NET version.");
			}

			private static bool TryGetDateTimeFieldAsLong(DateTime dateTime, out long dateData) {
				string[] fieldNames = { "dateData", "_dateData" };
				foreach (var name in fieldNames) {
					try {
						dateData = InternalReflector<DateTime>.GetField<long>(dateTime, name);
						return true;
					} catch { }
				}
				dateData = 0;
				return false;
			}

			private static bool TryGetDateTimeFieldAsULong(DateTime dateTime, out ulong dateData) {
				string[] fieldNames = { "dateData", "_dateData" };
				foreach (var name in fieldNames) {
					try {
						dateData = InternalReflector<DateTime>.GetField<ulong>(dateTime, name);
						return true;
					} catch { }
				}
				dateData = 0;
				return false;
			}
		}

		public class ListFieldTests {
			[Fact]
			public void GetListInternalSizeField() {
				var list = new List<int> { 1, 2, 3, 4, 5 };
				
				// Get the internal _size field
				var size = InternalReflector<List<int>>.GetField<int>(list, "_size");
				
				Assert.Equal(5, size);
				Assert.Equal(list.Count, size);
			}

			[Fact]
			public void SetListInternalSizeField() {
				var list = new List<int> { 1, 2, 3, 4, 5 };
				var originalCount = list.Count;
				
				// Modify the internal _size field
				InternalReflector<List<int>>.SetField(list, "_size", 3);
				
				// Demonstrate the effect - count is now 3
				Assert.Equal(3, list.Count);
				Assert.NotEqual(originalCount, list.Count);
			}

			[Fact]
			public void GetListInternalItemsField() {
				var list = new List<string> { "alpha", "beta", "gamma" };
				
				// Get the internal _items array
				var items = InternalReflector<List<string>>.GetField<string[]>(list, "_items");
				
				Assert.NotNull(items);
				Assert.True(items.Length >= 3); // Internal array is usually bigger than count
				
				// Verify the actual items are there
				Assert.Equal("alpha", items[0]);
				Assert.Equal("beta", items[1]);
				Assert.Equal("gamma", items[2]);
			}

			[Fact]
			public void AccessListMethodsDirectly() {
				var list = new List<int>();
				
				// Call the EnsureCapacity method directly (internal)
				InternalReflector<List<int>>.Call(list, "EnsureCapacity", 100);
				
				// Verify it worked by checking capacity
				Assert.True(list.Capacity >= 100);
			}
		}

		public class MemoryStreamFieldTests {
			[Fact]
			public void GetMemoryStreamInternalBufferField() {
				var data = new byte[] { 10, 20, 30, 40, 50 };
				var stream = new MemoryStream(data);
				
				// Get the internal _buffer field
				var buffer = InternalReflector<MemoryStream>.GetField<byte[]>(stream, "_buffer");
				
				Assert.NotNull(buffer);
				Assert.True(buffer.Length >= data.Length);
				
				// Verify content
				Assert.Equal(10, buffer[0]);
				Assert.Equal(20, buffer[1]);
				Assert.Equal(30, buffer[2]);
				Assert.Equal(40, buffer[3]);
				Assert.Equal(50, buffer[4]);
			}

			[Fact]
			public void SetMemoryStreamInternalPositionField() {
				var data = new byte[] { 10, 20, 30, 40, 50 };
				var stream = new MemoryStream(data);
				var originalPosition = stream.Position;
				
				// Set internal _position field directly
				InternalReflector<MemoryStream>.SetField(stream, "_position", 3);
				
				// Verify the position changed
				Assert.Equal(3, stream.Position);
				Assert.NotEqual(originalPosition, stream.Position);
				
				// Verify we can read from the new position
				var nextByte = stream.ReadByte();
				Assert.Equal(40, nextByte);
			}

			[Fact]
			public void VerifyMemoryStreamInternalWritableField() {
				var stream = new MemoryStream();
				
				// Get the internal _expandable field (determines if stream can expand)
				var expandable = InternalReflector<MemoryStream>.GetField<bool>(stream, "_expandable");
				
				Assert.True(expandable); // MemoryStream should be expandable
			}
		}

		public class ThreadFieldTests {
			[Fact]
			public void GetCurrentThreadInternalIdField() {
				var thread = Thread.CurrentThread;
				int internalId;
				if (!TryGetThreadIdField(thread, out internalId)) {
					throw new Xunit.Sdk.XunitException("No known internal Thread ID field found for this .NET version.");
				}
				Assert.Equal(thread.ManagedThreadId, internalId);
			}

			private static bool TryGetThreadIdField(Thread thread, out int id) {
				string[] fieldNames = { "m_ManagedThreadId", "_managedThreadId" };
				foreach (var name in fieldNames) {
					try {
						id = InternalReflector<Thread>.GetField<int>(thread, name);
						return true;
					} catch { }
				}
				id = 0;
				return false;
			}
		}

		public class ExceptionFieldTests {
			[Fact]
			public void AccessExceptionInternalStackTraceField() {
				Exception exception;
				try {
					throw new InvalidOperationException("Test exception");
				}
				catch (Exception ex) {
					exception = ex;
				}
				
				// The StackTrace property uses internal fields
				var stackTrace = exception.StackTrace;
				
				Assert.NotNull(stackTrace);
				Assert.Contains("AccessExceptionInternalStackTraceField", stackTrace);
			}

			[Fact]
			public void SetExceptionInternalMessageField() {
				var exception = new InvalidOperationException("Original message");
				
				// Set the internal message field
				InternalReflector<Exception>.SetField(exception, "_message", "Modified message");
				
				// Verify the change through public property
				Assert.Contains("Modified message", exception.Message);
			}
		}

		public class ErrorHandlingTests {
			[Fact]
			public void GetNonExistentFieldThrowsInvalidOperationException() {
				var list = new List<int>();
				
				Assert.Throws<InvalidOperationException>(() =>
					InternalReflector<List<int>>.GetField<string>(list, "NonExistentField"));
			}

			[Fact]
			public void SetNonExistentFieldThrowsInvalidOperationException() {
				var list = new List<int>();
				
				Assert.Throws<InvalidOperationException>(() =>
					InternalReflector<List<int>>.SetField(list, "NonExistentField", "value"));
			}

			[Fact]
			public void GetNonExistentPropertyThrowsInvalidOperationException() {
				var list = new List<int>();
				
				Assert.Throws<InvalidOperationException>(() =>
					InternalReflector<List<int>>.GetProperty<string>(list, "NonExistentProperty"));
			}

			[Fact]
			public void CallNonExistentMethodThrowsInvalidOperationException() {
				var list = new List<int>();
				
				Assert.Throws<InvalidOperationException>(() =>
					InternalReflector<List<int>>.Call(list, "NonExistentMethod"));
			}

			[Fact]
			public void NullMemberNameThrowsArgumentException() {
				var list = new List<int>();
				
				Assert.Throws<ArgumentException>(() =>
					InternalReflector<List<int>>.GetField<string>(list, null!));
				
				Assert.Throws<ArgumentException>(() =>
					InternalReflector<List<int>>.SetField(list, null!, "value"));
				
				Assert.Throws<ArgumentException>(() =>
					InternalReflector<List<int>>.Call(list, null!));
			}
		}

		public class StaticMemberTests {
			[Fact]
			public void TestStaticMethodAccessThroughExceptionHandling() {
				// Test error handling for non-existent static methods
				Assert.Throws<InvalidOperationException>(() =>
					InternalReflector<string>.Call("NonExistentStaticMethod"));
			}

			[Fact]
			public void TestReflectorErrorHandling() {
				// Demonstrate InternalReflector properly handles missing static fields
				Assert.Throws<InvalidOperationException>(() =>
					InternalReflector<DateTime>.GetField<int[]>("NonExistentStaticField"));
			}
		}

		public class ComplexScenariosTests {
			[Fact]
			public void ModifyListThroughInternalMechanismsAffectsBehavior() {
				var list = new List<string> { "a", "b", "c" };
				
				// Get internal _size and _items
				var originalSize = InternalReflector<List<string>>.GetField<int>(list, "_size");
				var items = InternalReflector<List<string>>.GetField<string[]>(list, "_items");
				
				Assert.Equal(3, originalSize);
				Assert.NotNull(items);
				Assert.Equal("a", items[0]);
				
				// Modify internal _size to be smaller
				InternalReflector<List<string>>.SetField(list, "_size", 1);
				
				// Now the list behaves as if it only has 1 item
				Assert.Single(list);
				Assert.Equal("a", list[0]);
				
				// Add something new - it should replace index 1 (where "b" was)
				list.Add("new");
				Assert.Equal(2, list.Count);
				Assert.Equal("new", list[1]);
			}

			[Fact]
			public void VerifyMemoryStreamInternalStateConsistency() {
				var data = new byte[] { 1, 2, 3, 4, 5 };
				var stream = new MemoryStream(data);
				
				// Read some data to move position
				var buffer = new byte[2];
				stream.Read(buffer, 0, 2);
				
				// Verify internal state matches public state
				var internalPosition = InternalReflector<MemoryStream>.GetField<int>(stream, "_position");
				var internalLength = InternalReflector<MemoryStream>.GetField<int>(stream, "_length");
				
				Assert.Equal(stream.Position, internalPosition);
				Assert.Equal(stream.Length, internalLength);
				
				// Modify position internally
				InternalReflector<MemoryStream>.SetField(stream, "_position", 0);
				Assert.Equal(0, stream.Position);
			}
		}

		public class InstanceTypingCompatibilityTests {
			private class BaseType {
			}

			private class DerivedType : BaseType {
#pragma warning disable 0414
				private int _hiddenField = 10;
#pragma warning restore 0414
				private int HiddenProperty { get; set; } = 20;

				private int HiddenMethod() {
					return 42;
				}
			}

			[Fact]
			public void CallAcceptsBaseTypedInstanceViaObjectOverload() {
				BaseType instance = new DerivedType();

				var result = InternalReflector<DerivedType>.Call(instance, "HiddenMethod");

				Assert.Equal(42, (int)result!);
			}

			[Fact]
			public void FieldAccessAcceptsBaseTypedInstanceViaObjectOverload() {
				BaseType instance = new DerivedType();

				var fieldValue = InternalReflector<DerivedType>.GetField<int>(instance, "_hiddenField");
				Assert.Equal(10, fieldValue);

				InternalReflector<DerivedType>.SetField(instance, "_hiddenField", 99);
				var updatedValue = InternalReflector<DerivedType>.GetField<int>(instance, "_hiddenField");
				Assert.Equal(99, updatedValue);
			}

			[Fact]
			public void PropertyAccessAcceptsBaseTypedInstanceViaObjectOverload() {
				BaseType instance = new DerivedType();

				var propertyValue = InternalReflector<DerivedType>.GetProperty<int>(instance, "HiddenProperty");
				Assert.Equal(20, propertyValue);

				InternalReflector<DerivedType>.SetProperty(instance, "HiddenProperty", 88);
				var updatedValue = InternalReflector<DerivedType>.GetProperty<int>(instance, "HiddenProperty");
				Assert.Equal(88, updatedValue);
			}
		}

		public class StaticInstanceResolutionTests {
			private class MixedMethodType {
				private static string Target(object value) {
					return "static";
				}

				private string Target(string value) {
					return "instance";
				}
			}

			[Fact]
			public void StaticCallUsesStaticMethodScope() {
				var result = InternalReflector<MixedMethodType>.Call("Target", new object[] { null! });
				Assert.Equal("static", (string)result!);
			}

			[Fact]
			public void InstanceCallUsesInstanceMethodScope() {
				var instance = new MixedMethodType();
				var result = InternalReflector<MixedMethodType>.Call(instance, "Target", new object[] { null! });
				Assert.Equal("instance", (string)result!);
			}
		}

		public class TypeReflectorStaticClassTests {
			private static class StaticOnlyType {
				private static int _count = 7;
				private static string Label { get; set; } = "initial";

				private static string Touch(int add) {
					_count += add;
					return Label + ":" + _count;
				}
			}

			[Fact]
			public void ForTypeCanAccessStaticClassMembers() {
				var reflector = InternalReflector.For(typeof(StaticOnlyType));

				var fieldBefore = reflector.GetField<int>("_count");
				Assert.Equal(7, fieldBefore);

				reflector.SetProperty("Label", "updated");
				var methodResult = reflector.Call("Touch", 3);
				Assert.Equal("updated:10", (string)methodResult!);

				var fieldAfter = reflector.GetField<int>("_count");
				var label = reflector.GetProperty<string>("Label");
				Assert.Equal(10, fieldAfter);
				Assert.Equal("updated", label);
			}
		}
	}
}
