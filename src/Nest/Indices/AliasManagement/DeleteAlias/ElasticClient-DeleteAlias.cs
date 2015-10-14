using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;

namespace Nest
{
	public partial interface IElasticClient
	{
		/// <summary>
		/// Delete an index alias
		/// http://www.elasticsearch.org/guide/en/elasticsearch/reference/current/indices-aliases.html#deleting
		/// </summary>
		/// <param name="deleteAliasRequest">A descriptor that describes the delete alias request</param>
		IAcknowledgedResponse DeleteAlias(IDeleteAliasRequest deleteAliasRequest);

		/// <inheritdoc/>
		Task<IAcknowledgedResponse> DeleteAliasAsync(IDeleteAliasRequest deleteAliasRequest);

		/// <inheritdoc/>
		IAcknowledgedResponse DeleteAlias(Indices indices, Names names, Func<DeleteAliasDescriptor, IDeleteAliasRequest> deleteAliasDescriptor = null);

		/// <inheritdoc/>
		Task<IAcknowledgedResponse> DeleteAliasAsync(Indices indices, Names names, Func<DeleteAliasDescriptor, IDeleteAliasRequest> deleteAliasDescriptor = null);
	}


	public partial class ElasticClient
	{
		/// <inheritdoc/>
		public IAcknowledgedResponse DeleteAlias(IDeleteAliasRequest deleteAliasRequest) => 
			this.Dispatcher.Dispatch<IDeleteAliasRequest, DeleteAliasRequestParameters, AcknowledgedResponse>(
				deleteAliasRequest,
				(p, d) => this.LowLevelDispatch.IndicesDeleteAliasDispatch<AcknowledgedResponse>(p)
			);

		/// <inheritdoc/>
		public Task<IAcknowledgedResponse> DeleteAliasAsync(IDeleteAliasRequest deleteAliasRequest) => 
			this.Dispatcher.DispatchAsync<IDeleteAliasRequest, DeleteAliasRequestParameters, AcknowledgedResponse, IAcknowledgedResponse>(
				deleteAliasRequest,
				(p, d) => this.LowLevelDispatch.IndicesDeleteAliasDispatchAsync<AcknowledgedResponse>(p)
			);

		/// <inheritdoc/>
		public IAcknowledgedResponse DeleteAlias(Indices indices, Names names, Func<DeleteAliasDescriptor, IDeleteAliasRequest> deleteAliasDescriptor = null) =>
			this.DeleteAlias(deleteAliasDescriptor.InvokeOrDefault(new DeleteAliasDescriptor(indices, names)));

		/// <inheritdoc/>
		public Task<IAcknowledgedResponse> DeleteAliasAsync(Indices indices, Names names, Func<DeleteAliasDescriptor, IDeleteAliasRequest> deleteAliasDescriptor = null) =>
			this.DeleteAliasAsync(deleteAliasDescriptor.InvokeOrDefault(new DeleteAliasDescriptor(indices, names)));
	}
}