﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Nest.Resolvers.Converters.Aggregations
{
	public class FilterAggregatorConverter : JsonConverter
	{
		public override bool CanRead { get { return true; } }
		public override bool CanWrite { get { return true; } }

		public override bool CanConvert(Type objectType)
		{
			return true; //only to be used with attribute or contract registration.
		}
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var f = value as IFilterAggregator;
			if (f == null || f.Filter == null)
			{
				writer.WriteNull();
				return;
			};

			serializer.Serialize(writer, f.Filter);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.StartObject) return null;
			var container = new QueryContainer();
			serializer.Populate(reader, container);
			var agg = new FilterAggregator();
			agg.Filter = container;

			return agg;
		}
	}

}
	