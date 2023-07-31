CREATE TABLE IF NOT EXISTS friends
(
    friend_one_id timeuuid,
    friend_two_id timeuuid,
    created_at    timestamp,
    PRIMARY KEY ( (friend_one_id), created_at )
) WITH CLUSTERING ORDER BY (created_at DESC) AND CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };

CREATE TABLE IF NOT EXISTS chat_groups
(
    id            timeuuid,
    creator_id    timeuuid,
    admin_ids     set<timeuuid>,
    created_at    timestamp,
    PRIMARY KEY ( (id), created_at )
) WITH CLUSTERING ORDER BY (created_at DESC) AND CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };

CREATE TABLE IF NOT EXISTS chat_group_members_count
(
    id            timeuuid,
    members_count counter,
    PRIMARY KEY ( id )
) WITH CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };

CREATE TABLE IF NOT EXISTS chat_messages
(
    id              timeuuid,
    chat_group_id   timeuuid,
    user_id         timeuuid,
    content         text,
    attachments     set<text>,
    metadata        map<text, text>,
    reaction_counts map<tinyint, bigint>,
    created_at      timestamp,
    updated_at      timestamp,
    updated         boolean,
    PRIMARY KEY ( (chat_group_id), created_at, id)
) WITH CLUSTERING ORDER BY (created_at DESC, id ASC) AND CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };

CREATE TABLE IF NOT EXISTS chat_messages_reply_count
(
    id          timeuuid,
    reply_count counter,
    PRIMARY KEY ( id)
) WITH CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };

CREATE TABLE IF NOT EXISTS chat_message_replies
(
    id              timeuuid,
    chat_group_id   timeuuid,
    reply_to        timeuuid,
    user_id         timeuuid,
    content         text,
    attachments     set<text>,
    metadata        map<text, text>,
    reaction_counts map<tinyint, bigint>,
    created_at      timestamp,
    updated_at      timestamp,
    updated         boolean,
    PRIMARY KEY ( (reply_to), created_at, id)
) WITH CLUSTERING ORDER BY (created_at DESC, id ASC) AND CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };

CREATE TABLE IF NOT EXISTS friend_invitations
(
    id         timeuuid,
    inviter_id timeuuid,
    invitee_id timeuuid,
    metadata   map<text, text>,
    status     tinyint,
    created_at timestamp,
    updated_at timestamp,
    updated    boolean,
    PRIMARY KEY ( (inviter_id), created_at)
) WITH CLUSTERING ORDER BY (created_at DESC) AND CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };

CREATE TABLE IF NOT EXISTS message_reactions
(
    id            timeuuid,
    message_id    timeuuid,
    group_chat_id timeuuid,
    user_id       timeuuid,
    reaction_type tinyint,
    metadata      map<text, text>,
    created_at    timestamp,
    updated_at    timestamp,
    updated       boolean,
    PRIMARY KEY ((message_id), created_at, id)
) WITH CLUSTERING ORDER BY (created_at DESC, id ASC) AND CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };

CREATE TABLE IF NOT EXISTS chat_group_members
(
    id            timeuuid,
    user_id       timeuuid,
    chat_group_id timeuuid,
    created_at    timestamp,
    metadata      map<text, text>,
    type          tinyint,
    PRIMARY KEY ( (chat_group_id), created_at, user_id )
) WITH CLUSTERING ORDER BY (created_at DESC, user_id ASC) AND CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };
