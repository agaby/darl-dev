using Darl.GraphQL.Process.Blazor.Connectivity;
using GraphQL.Validation;
using GraphQLParser.AST;
using Microsoft.Extensions.Caching.Memory;
using Azure.Data.Tables;

namespace Darl.GraphQL.Blazor.Server.Helpers
{
    public class LicenseValidationRule : IValidationRule
    {
        static readonly string objectIdClaimText = @"http://schemas.microsoft.com/identity/claims/objectidentifier";
        static readonly string tenantIdClaimText = @"http://schemas.microsoft.com/identity/claims/tenantid";

        private readonly IConfiguration _config;
        private readonly IMemoryCache _mem;
        private readonly IKGTranslation _trans;
        private readonly int cacheDurationMinutes = 30;
        private readonly bool allowAnonymous = false;
        private readonly TableClient _tableClient;
        private readonly ILogger<LicenseValidationRule> _logger;


        public LicenseValidationRule(IMemoryCache memoryCache, IConfiguration config, IKGTranslation trans, ILogger<LicenseValidationRule> logger)
        {
            _mem = memoryCache;
            _config = config;
            allowAnonymous = _config.GetValue<bool?>("allowAnonymous") ?? false;
            _trans = trans;
            _tableClient = new TableClient(_config["AppSettings:StorageConnectionString"], _config["AppSourceRecordTable"]);
            _logger = logger;
        }

        public async ValueTask<INodeVisitor> ValidateAsync(ValidationContext context)
        {
            return  new MatchingNodeVisitor<GraphQLOperationDefinition>(async (op, context) =>  
            {
                if (!allowAnonymous)
                {
                    if (!context!.User!.Identity!.IsAuthenticated)
                    {
                        context.ReportError(new ValidationError("Not authenticated. Please log in. "));
                    }
                    else
                    {
                        var userId = _trans.GetCurrentUserIdFromClaim(context.User);
                        var tenantId = _trans.GetCurrentTenantIdFromClaim(context.User);
                        var host = _trans.GetCurrentHostFromClaim(context.User);
                        if(string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(host)) 
                        {
                            context.ReportError(new ValidationError("Not authenticated. Please log in. "));
                        }
                        else
                        {
                            try
                            {
                                //check in cache for response
                                _mem.TryGetValue(userId, out AppSourceRecord? record);
                                if (record != null)
                                {
                                    if (!record.enabled)
                                    {
                                        context.ReportError(new ValidationError("Not authorized. Please obtain a license. "));
                                    }
                                }
                                else //if not in cache 
                                {
                                    if (_config.GetSection("PossibleHosts").Get<List<string>>()!.Contains(host))//  if host is recognized as Thinkbase.dev
                                    {
                                        //      add/overwrite existing record
                                        var entity = new TableEntity(userId, tenantId) { { "enabled", true } };
                                        await _tableClient.UpsertEntityAsync(entity);
                                        //      add to cache
                                        _mem.Set<AppSourceRecord>(userId, new AppSourceRecord { userId = userId, tenantId = tenantId, enabled = true }, TimeSpan.FromMinutes(cacheDurationMinutes));
                                        _logger.LogInformation($"Added an AppSourceRecord for userId {userId}");
                                    }
                                    else //API host
                                    {
                                        //      if record doesn't exist or record does exist but not enabled
                                        var res = await _tableClient.GetEntityAsync<TableEntity>(userId, tenantId);
                                        if (res == null || !((res.Value["enabled"] as bool?) ?? false))
                                        {
                                            _logger.LogInformation($"Refused access to userId {userId} at host {host}");
                                            context.ReportError(new ValidationError("Not authorized. Please obtain a license. "));
                                        }

                                    }
                                }
                            }
                            catch(Exception ex)
                            {
                                context.ReportError(new ValidationError("Internal Error. Please notify support. "));
                                _logger.LogError($"Failed to create/load appUserTable item", ex);
                            }

                        }

                    }
                }
            });
        }
    }
}
