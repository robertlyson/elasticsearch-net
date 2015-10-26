﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;
using FluentAssertions;
using Nest;
using Tests.Framework;
using Tests.Framework.Integration;
using Tests.Framework.MockData;
using Xunit;

namespace Tests.Document.Multiple.Bulk
{
	[Collection(IntegrationContext.Indexing)]
	public class BulkApiTests : ApiIntegrationTestBase<IBulkResponse, IBulkRequest, BulkDescriptor, BulkRequest>
	{
		public BulkApiTests(IndexingCluster cluster, EndpointUsage usage) : base(cluster, usage) { }
		protected override LazyResponses ClientUsage() => Calls(
			fluent: (client, f) => client.Bulk(f),
			fluentAsync: (client, f) => client.BulkAsync(f),
			request: (client, r) => client.Bulk(r),
			requestAsync: (client, r) => client.BulkAsync(r)
		);

		protected override bool ExpectIsValid => true;
		protected override int ExpectStatusCode => 200;
		protected override HttpMethod HttpMethod => HttpMethod.POST;
		protected override string UrlPath => $"/{CallIsolatedValue}/_bulk";

		protected override bool SupportsDeserialization => false;

		protected override object ExpectJson { get; } = new object[]
		{
			new Dictionary<string, object>{ { "index", new {  _type = "project", _id = Project.Instance.Name } } },
			Project.InstanceAnonymous,
			new Dictionary<string, object>{ { "update", new { _type="project", _id = Project.Instance.Name } } },
			new { leadDeveloper = new { firstName = "martijn" } },
			new Dictionary<string, object>{ { "create", new { _type="project", _id = Project.Instance.Name + "1" } } },
			Project.InstanceAnonymous,
			new Dictionary<string, object>{ { "delete", new { _type="project", _id = Project.Instance.Name + "1" } } },
		};

		protected override Func<BulkDescriptor, IBulkRequest> Fluent => d => d
			.Index(CallIsolatedValue)
			.Index<Project>(b => b.Document(Project.Instance))
			.Update<Project, object>(b => b.Doc(new { leadDeveloper = new { firstName = "martijn" } }).Id(Project.Instance.Name))
			.Create<Project>(b => b.Document(Project.Instance).Id(Project.Instance.Name + "1"))
			.Delete<Project>(b=>b.Id(Project.Instance.Name + "1"));
			

		protected override BulkRequest Initializer => new BulkRequest(CallIsolatedValue)
		{
			Operations = new List<IBulkOperation>
			{
				new BulkIndexOperation<Project>(Project.Instance),
				new BulkUpdateOperation<Project, object>(Project.Instance)
				{
					Doc = new { leadDeveloper = new { firstName = "martijn" } }
				},
				new BulkCreateOperation<Project>(Project.Instance)
				{
					Id = Project.Instance.Name + "1"
				},
				new BulkDeleteOperation<Project>(Project.Instance + "1"),
			}
		};
		[I] public async Task Response() => await this.AssertOnAllResponses(r =>
		{
			r.Took.Should().BeGreaterThan(0);
			r.Errors.Should().BeFalse();
			r.ItemsWithErrors.Should().NotBeNull().And.BeEmpty();
			r.Items.Should().NotBeEmpty();
			foreach (var item in r.Items)
			{
				item.Index.Should().Be(CallIsolatedValue);
				item.Type.Should().Be("project");
				item.Status.Should().BeGreaterThan(100);
				item.Version.Should().BeGreaterThan(0);
				item.Id.Should().NotBeNullOrWhiteSpace();
				item.IsValid.Should().BeTrue();
				item.Shards.Should().NotBeNull();
				item.Shards.Total.Should().Be(2);
				item.Shards.Successful.Should().BeGreaterThan(0);
			}

			var p1 = this.Client.Source<Project>(Project.Instance.Name, p=>p.Index(CallIsolatedValue));
			p1.LeadDeveloper.FirstName.Should().Be("martijn");
		});
	}
}
