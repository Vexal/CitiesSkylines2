using Colossal.UI.Binding;
using System;
using System.Collections.Generic;

namespace BuildingUsageTracker
{
	public class BasicJsonWriter : IJsonWriter
	{
		// Dedicated fields for Name serialization
		public string TypeName { get; private set; }
		public string NameId { get; private set; }
		public string CustomName { get; private set; }
		public Dictionary<string, string> NameArgs { get; } = new Dictionary<string, string>();
		public string Result { get; private set; }

		private string currentProperty = null;
		private bool inMap = false;
		private string pendingMapKey = null;

		public string debugName => "BasicJsonWriter";

		public void ArrayBegin(uint size) { /* not used for Name */ }
		public void ArrayEnd() { }

		public void MapBegin(uint size)
		{
			inMap = true;
			pendingMapKey = null;
		}

		public void MapEnd()
		{
			inMap = false;
			pendingMapKey = null;
		}

		public void PropertyName(string name)
		{
			currentProperty = name;
		}

		public void TypeBegin(string name)
		{
			TypeName = name;
			// clear fields
			NameId = null;
			CustomName = null;
			NameArgs.Clear();
			Result = null;
		}

		public void TypeEnd()
		{
			// build a minimal JSON representation for compatibility
			try
			{
				if (TypeName == null) { Result = null; return; }
				if (TypeName.Contains("CustomName"))
				{
					Result = "{\"$type\":\"names.CustomName\",\"name\":\"" + Escape(CustomName) + "\"}";
					return;
				}
				if (TypeName.Contains("LocalizedName"))
				{
					Result = "{\"$type\":\"names.LocalizedName\",\"nameId\":\"" + Escape(NameId) + "\"}";
					return;
				}
				if (TypeName.Contains("FormattedName"))
				{
					var argsJson = "{";
					bool first = true;
					foreach (var kv in NameArgs)
					{
						if (!first) argsJson += ",";
						argsJson += "\"" + Escape(kv.Key) + "\":\"" + Escape(kv.Value) + "\"";
						first = false;
					}
					argsJson += "}";
					Result = "{\"$type\":\"names.FormattedName\",\"nameId\":\"" + Escape(NameId) + "\",\"nameArgs\":" + argsJson + "}";
					return;
				}
				Result = "{}";
			}
			catch { Result = null; }
		}

		public void Write(bool value) { /* not used for Name */ }
		public void Write(int value) { /* not used for Name */ }
		public void Write(uint value) { /* not used for Name */ }
		public void Write(long value) { /* not used for Name */ }
		public void Write(ulong value) { /* not used for Name */ }
		public void Write(float value) { /* not used for Name */ }
		public void Write(double value) { /* not used for Name */ }

		public void Write(string value)
		{
			if (inMap)
			{
				// map writes come in pairs: key then value
				if (pendingMapKey == null)
				{
					pendingMapKey = value ?? string.Empty;
				}
				else
				{
					NameArgs[pendingMapKey ?? string.Empty] = value ?? string.Empty;
					pendingMapKey = null;
				}
				return;
			}

			if (currentProperty != null)
			{
				if (string.Equals(currentProperty, "name", StringComparison.OrdinalIgnoreCase))
				{
					CustomName = value;
				}
				else if (string.Equals(currentProperty, "nameId", StringComparison.OrdinalIgnoreCase))
				{
					NameId = value;
				}
				// reset
				currentProperty = null;
				return;
			}

			// If nothing matched, ignore
		}

		public void WriteNull()
		{
			Write((string)null);
		}

		private static string Escape(string s)
		{
			if (s == null) return string.Empty;
			return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
		}
	}
}
