﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;

namespace Nest
{
	public partial interface IElasticClient
	{
		/// <summary>
		/// Indices level stats provide statistics on different operations happening on an index. The API provides statistics on 
		/// the index level scope (though most stats can also be retrieved using node level scope).
		/// <para> </para>http://www.elasticsearch.org/guide/en/elasticsearch/reference/current/indices-stats.html
		/// </summary>
		/// <param name="selector">Optionaly further describe the indices stats operation</param>
		IIndicesStatsResponse IndicesStats(Indices indices, Func<IndicesStatsDescriptor, IIndicesStatsRequest> selector = null);

		/// <inheritdoc/>
		IIndicesStatsResponse IndicesStats(IIndicesStatsRequest indicesStatsRequest);

		/// <inheritdoc/>
		Task<IIndicesStatsResponse> IndicesStatsAsync(Indices indices, Func<IndicesStatsDescriptor, IIndicesStatsRequest> selector = null);

		/// <inheritdoc/>
		Task<IIndicesStatsResponse> IndicesStatsAsync(IIndicesStatsRequest indicesStatsRequest);

	}

	public partial class ElasticClient
	{
		/// <inheritdoc/>
		public IIndicesStatsResponse IndicesStats(Indices indices, Func<IndicesStatsDescriptor, IIndicesStatsRequest> selector = null) =>
			this.IndicesStats(selector.InvokeOrDefault(new IndicesStatsDescriptor().Index(indices)));

		/// <inheritdoc/>
		public IIndicesStatsResponse IndicesStats(IIndicesStatsRequest statsRequest) => 
			this.Dispatcher.Dispatch<IIndicesStatsRequest, IndicesStatsRequestParameters, IndicesStatsResponse>(
				statsRequest,
				(p, d) => this.LowLevelDispatch.IndicesStatsDispatch<IndicesStatsResponse>(p)
			);

		/// <inheritdoc/>
		public Task<IIndicesStatsResponse> IndicesStatsAsync(Indices indices, Func<IndicesStatsDescriptor, IIndicesStatsRequest> selector = null) => 
			this.IndicesStatsAsync(selector.InvokeOrDefault(new IndicesStatsDescriptor().Index(indices)));

		/// <inheritdoc/>
		public Task<IIndicesStatsResponse> IndicesStatsAsync(IIndicesStatsRequest statsRequest) => 
			this.Dispatcher.DispatchAsync<IIndicesStatsRequest, IndicesStatsRequestParameters, IndicesStatsResponse, IIndicesStatsResponse>(
				statsRequest,
				(p, d) => this.LowLevelDispatch.IndicesStatsDispatchAsync<IndicesStatsResponse>(p)
			);
	}
}