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
		/// Add a single index alias
		/// http://www.elasticsearch.org/guide/en/elasticsearch/reference/current/indices-aliases.html#alias-adding
		/// </summary>
		/// <param name="putAliasRequest">A descriptor that describes the put alias request</param>
		IAcknowledgedResponse PutAlias(IPutAliasRequest putAliasRequest);

		/// <inheritdoc/>
		Task<IAcknowledgedResponse> PutAliasAsync(IPutAliasRequest putAliasRequest);

		/// <inheritdoc/>
		IAcknowledgedResponse PutAlias(Indices indices, Name alias, Func<PutAliasDescriptor, IPutAliasRequest> putAliasSelector = null); 

		/// <inheritdoc/>
		Task<IAcknowledgedResponse> PutAliasAsync(Indices indices, Name alias, Func<PutAliasDescriptor, IPutAliasRequest> putAliasSelector = null);
	}

	public partial class ElasticClient
	{
		/// <inheritdoc/>
		public IAcknowledgedResponse PutAlias(IPutAliasRequest putAliasRequest) => 
			this.Dispatcher.Dispatch<IPutAliasRequest, PutAliasRequestParameters, AcknowledgedResponse>(
				putAliasRequest,
				this.LowLevelDispatch.IndicesPutAliasDispatch<AcknowledgedResponse>
			);

		/// <inheritdoc/>
		public Task<IAcknowledgedResponse> PutAliasAsync(IPutAliasRequest putAliasRequest) => 
			this.Dispatcher.DispatchAsync<IPutAliasRequest, PutAliasRequestParameters, AcknowledgedResponse, IAcknowledgedResponse>(
				putAliasRequest,
				this.LowLevelDispatch.IndicesPutAliasDispatchAsync<AcknowledgedResponse>
			);

		/// <inheritdoc/>
		public IAcknowledgedResponse PutAlias(Indices indices, Name alias, Func<PutAliasDescriptor, IPutAliasRequest> putAliasSelector = null) =>
			this.PutAlias(putAliasSelector.InvokeOrDefault(new PutAliasDescriptor(indices, alias)));

		/// <inheritdoc/>
		public Task<IAcknowledgedResponse> PutAliasAsync(Indices indices, Name alias, Func<PutAliasDescriptor, IPutAliasRequest> putAliasSelector = null) =>
			this.PutAliasAsync(putAliasSelector.InvokeOrDefault(new PutAliasDescriptor(indices, alias)));
	}
}