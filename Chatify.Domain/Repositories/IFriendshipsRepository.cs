﻿using Chatify.Domain.Common;
using Chatify.Domain.Entities;

namespace Chatify.Domain.Repositories;

public interface IFriendshipsRepository : IDomainRepository<FriendsRelation, Guid>
{
   Task<List<User>> AllForUser(Guid userId, CancellationToken cancellationToken = default);
}