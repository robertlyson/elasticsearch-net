using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Primitives;
using Nest;
using Tests.Framework;
using Tests.Framework.Integration;
using Tests.Framework.MockData;
using Xunit;

namespace Tests.Indices.AliasManagement
{
	[Collection(IntegrationContext.Indexing)]
	public class AliasManagementCrudTests
		: CrudTestBase<IAcknowledgedResponse, IGetAliasesResponse, IAcknowledgedResponse>
	{
		private readonly string _aliasName = "happyAlias";
		private readonly string _updatedAliasName = "sadAlias";

		public AliasManagementCrudTests(IndexingCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool SupportsDeletes => true;

		protected override LazyResponses Create() => Calls<PutAliasDescriptor, PutAliasRequest, IPutAliasRequest, IAcknowledgedResponse>(
			CreateInitializer,
			CreateFluent,
			fluent: (s, c, f) =>
			{
				if (!c.IndexExists(s).Exists) c.CreateIndex(s);
				var response = c.PutAlias(s, _aliasName, f);
				return response;
			},
			fluentAsync: async (s, c, f) =>
			{
				if (!c.IndexExists(s).Exists) c.CreateIndex(s);
				var response = await c.PutAliasAsync(s, _aliasName, f);
				return response;
			},
			request: (s, c, r) =>
			{
				if (!c.IndexExists(s).Exists) c.CreateIndex(s);
				var response = c.PutAlias(r);
				return response;
			},
			requestAsync: async (s, c, r) =>
			{
				if(!c.IndexExists(s).Exists) c.CreateIndex(s);
				var response = await c.PutAliasAsync(r);
				return response;
			}
		);

		private PutAliasRequest CreateInitializer(string index) => 
			new PutAliasRequest(index, _aliasName);

		private IPutAliasRequest CreateFluent(string index, PutAliasDescriptor d) => d;

		protected override LazyResponses Read() => Calls<GetAliasDescriptor, GetAliasRequest, IGetAliasRequest, IGetAliasesResponse>(
			ReadInitializer,
			ReadFluent,
			fluent: (s, c, f) => c.GetAlias(f),
			fluentAsync: (s, c, f) => c.GetAliasAsync(f),
			request: (s, c, r) => c.GetAlias(r),
			requestAsync: (s, c, r) => c.GetAliasAsync(r)
		);

		private GetAliasRequest ReadInitializer(string index) =>
			new GetAliasRequest(index, _aliasName);

		private IGetAliasRequest ReadFluent(string index, GetAliasDescriptor d) => 
			d.Index(index).Alias(_aliasName);

		protected override LazyResponses Update() => Calls<PutAliasDescriptor, PutAliasRequest, IPutAliasRequest, IAcknowledgedResponse>(
			UpdateInitializer,
			UpdateFluent,
			fluent: (s, c, f) =>
			{
				var deleteAlias = c.DeleteAlias(s, _aliasName);
				var response = c.PutAlias(s, _updatedAliasName, f);
				return response;
			},
			fluentAsync: async (s, c, f) =>
			{
				c.DeleteAlias(s, _aliasName);
				var response = await c.PutAliasAsync(s, _updatedAliasName, f);
				return response;
			},
			request: (s, c, r) =>
			{
				c.DeleteAlias(s, _aliasName);
				var response = c.PutAlias(r);
				return response;
			},
			requestAsync: async (s, c, r) =>
			{
				c.DeleteAlias(s, _aliasName);
				var response = await c.PutAliasAsync(r);
				return response;
			}
		);

		private PutAliasRequest UpdateInitializer(string index) => 
			new PutAliasRequest(index, _updatedAliasName);

		private IPutAliasRequest UpdateFluent(string index, PutAliasDescriptor d) => d.Index(index);

		protected override LazyResponses Delete() => Calls<DeleteAliasDescriptor, DeleteAliasRequest, IDeleteAliasRequest, IAcknowledgedResponse>(
			DeleteInitializer,
			DeleteFluent,
			fluent: (s, c, f) => c.DeleteAlias(s, _updatedAliasName, f),
			fluentAsync: (s, c, f) => c.DeleteAliasAsync(s, _updatedAliasName, f),
			request: (s, c, r) => c.DeleteAlias(r),
			requestAsync: (s, c, r) => c.DeleteAliasAsync(r)
		);

		private DeleteAliasRequest DeleteInitializer(string index) => 
			new DeleteAliasRequest(index, _updatedAliasName);

		private IDeleteAliasRequest DeleteFluent(string index, DeleteAliasDescriptor d) => d;

		[I] protected async Task OriginAliasDoesntExistAfterUpdate() => await this.AssertOnGetAfterUpdate(r =>
			ContainsAlias(r, _aliasName).Should().BeFalse()
		);

		[I] protected async Task AliasWasUpdated() => await this.AssertOnGetAfterUpdate(r =>
			ContainsAlias(r, _updatedAliasName).Should().BeTrue()
		);

		[I] protected async Task AliasDoesntExistAfterDelete() => await this.AssertOnDelete(r =>
			Client.AliasExists(d => d.Name(_updatedAliasName)).Exists.Should().BeFalse()
		);

		private bool ContainsAlias(IGetAliasesResponse r, string aliasName)
		{
			return r.Indices.FirstOrDefault().Value
				.Select(x => x.Name)
				.Contains(aliasName);
		}
	}
}
