using AspNetCore.Identity.Cassandra.Models;

namespace Chatify.Infrastructure.Data.Mappings;

public class RoleMapping : Cassandra.Mapping.Mappings
{
    private const string RolesTableName = "roles";

    public RoleMapping()
    {
        For<CassandraIdentityRole>()
            .TableName(RolesTableName)
            .KeyspaceName(Constants.KeyspaceName)
            .PartitionKey(r => r.Id);
    }
    
}