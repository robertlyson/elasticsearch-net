﻿using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Nest;
using Tests.Framework;
using Tests.Framework.Integration;
using Xunit;

namespace Tests.Indices.IndexManagement.OpenCloseIndex.OpenIndex
{
	[Collection(IntegrationContext.Indexing)]
	public class OpenIndexApiTests 
		: ApiIntegrationTestBase<IIndicesOperationResponse, IOpenIndexRequest, OpenIndexDescriptor, OpenIndexRequest>
	{
		public OpenIndexApiTests(IndexingCluster cluster, EndpointUsage usage) : base(cluster, usage) { }
		protected override LazyResponses ClientUsage() => Calls(
			fluent: (client, f) => client.OpenIndex(CallIsolatedValue, f),
			fluentAsync: (client, f) => client.OpenIndexAsync(CallIsolatedValue, f),
			request: (client, r) => client.OpenIndex(r),
			requestAsync: (client, r) => client.OpenIndexAsync(r)
			);

		protected override bool ExpectIsValid => true;
		protected override int ExpectStatusCode => 200;
		protected override HttpMethod HttpMethod => HttpMethod.POST;
		protected override string UrlPath => $"/{CallIsolatedValue}/_open?ignore_unavailable=true";

		protected override OpenIndexDescriptor NewDescriptor() => new OpenIndexDescriptor(CallIsolatedValue);

		protected override Func<OpenIndexDescriptor, IOpenIndexRequest> Fluent => d => d
			.IgnoreUnavailable();

		protected override OpenIndexRequest Initializer => new OpenIndexRequest(CallIsolatedValue)
		{
			IgnoreUnavailable = true
		};
}
}