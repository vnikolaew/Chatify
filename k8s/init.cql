CREATE KEYSPACE IF NOT EXISTS chatify
        WITH REPLICATION = { 'class': 'SimpleStrategy', 'replication_factor': 3};

USE chatify;

// UDTs:
create type if not exists lockoutinfo
    (
        enddate           timestamp,
        enabled           boolean,
        accessfailedcount int
    );

create type if not exists logininfo
    (
        loginprovider       text,
        providerkey         text,
        providerdisplayname text
    );

create type if not exists phoneinfo
    (
        number           text,
        confirmationtime timestamp
    );

create type if not exists tokeninfo
    (
        loginprovider text,
        name          text,
        value         text
    );

CREATE TYPE IF NOT EXISTS attachment_info
    (
        absolute_url text,
        filename     text,
        type         text,
        metadata     map<text,text>
    );

CREATE TYPE IF NOT EXISTS media
    (
        id        uuid,
        media_url text,
        file_name text,
        type      text,
        metadata  frozen<map<text, text>>
    );

CREATE TYPE IF NOT EXISTS message_pin
    (
        message_id uuid,
        pinner_id  uuid,
        created_at timestamp,
    );

CREATE TYPE IF NOT EXISTS message_replier_info
    (
        user_id             uuid,
        username            text,
        profile_picture_url text,
    );

CREATE TABLE IF NOT EXISTS users
(
    id                    uuid,
    created_at            timestamp,
    username              text,
    banner_pictures       set<frozen<media>>,
    device_ips            set<blob>,
    display_name          text,
    user_handle           text,
    email                 text,
    emailconfirmationtime timestamp,
    last_login            timestamp,
    lockout               lockoutinfo,
    logins                list<frozen<logininfo>>,
    metadata              map<text, text>,
    normalizedemail       text,
    normalizedusername    text,
    passwordhash          text,
    phone                 phoneinfo,
    phone_numbers         set<text>,
    profile_picture       media,
    roles                 list<text>,
    securitystamp         text,
    status                tinyint,
    tokens                list<frozen<tokeninfo>>,
    twofactorenabled      boolean,
    updated_at            timestamp,
    PRIMARY KEY ((id))
);

CREATE INDEX IF NOT EXISTS users_by_user_handle ON users (user_handle);

CREATE MATERIALIZED VIEW IF NOT EXISTS users_by_email
AS
SELECT *
FROM chatify.users
WHERE NormalizedEmail IS NOT NULL
  AND id IS NOT NULL
PRIMARY KEY (NormalizedEmail, id);

CREATE MATERIALIZED VIEW IF NOT EXISTS users_by_username AS
SELECT *
FROM chatify.users
WHERE NormalizedUserName IS NOT NULL
  AND Id IS NOT NULL
PRIMARY KEY (NormalizedUserName, Id);

create table if not exists roles
(
    id             uuid primary key,
    name           text,
    normalizedname text
);

CREATE MATERIALIZED VIEW IF NOT EXISTS roles_by_name AS
SELECT *
FROM chatify.roles
WHERE NormalizedName IS NOT NULL
  AND Id IS NOT NULL
PRIMARY KEY (NormalizedName, Id);

CREATE MATERIALIZED VIEW IF NOT EXISTS userclaims_by_type_and_value AS
    SELECT * FROM chatify.userclaims WHERE Type IS NOT NULL
    AND Value IS NOT NULL AND userid IS NOT NULL PRIMARY KEY ((Type, Value), UserId);

create table if not exists userclaims
(
    userid uuid,
    type   text,
    value  text,
    primary key (userid, type, value)
);

CREATE TABLE IF NOT EXISTS friends
(
    id            uuid,
    friend_one_id uuid,
    group_id      uuid,
    friend_two_id uuid,
    created_at    timestamp,
    PRIMARY KEY ( (friend_one_id), created_at )
) WITH CLUSTERING ORDER BY (created_at DESC) AND CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };

CREATE TABLE IF NOT EXISTS chat_groups
(
    id              uuid,
    creator_id      uuid,
    name            text,
    about           text,
    picture         media,
    metadata        map<text, text>,
    pinned_messages map<uuid, frozen<message_pin>>,
    updated_at      timestamp,
    admin_ids       set<uuid>,
    created_at      timestamp,
    PRIMARY KEY ( (id), created_at )
) WITH CLUSTERING ORDER BY (created_at DESC) AND CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };

CREATE TABLE IF NOT EXISTS chat_group_members_count
(
    chat_group_id uuid,
    members_count counter,
    PRIMARY KEY ( chat_group_id )
) WITH CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };

CREATE TABLE IF NOT EXISTS chat_messages
(
    id              uuid,
    chat_group_id   uuid,
    user_id         uuid,
    content         text,
    attachments     set<frozen<media>>,
    metadata        map<text, text>,
    reaction_counts map<bigint, bigint>,
    created_at      timestamp,
    updated_at      timestamp,
    updated         boolean,
    PRIMARY KEY ( (chat_group_id), created_at)
) WITH CLUSTERING ORDER BY (created_at DESC) AND CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };

CREATE MATERIALIZED VIEW IF NOT EXISTS chat_messages_by_id
AS
SELECT *
FROM chat_messages
WHERE id IS NOT NULL
  AND chat_group_id IS NOT NULL
  AND created_at IS NOT NULL
PRIMARY KEY ( id, chat_group_id, created_at );

CREATE INDEX IF NOT EXISTS chat_messages_id_idx ON chat_messages (id);

CREATE TABLE IF NOT EXISTS chat_message_drafts
(
    id              uuid,
    chat_group_id   uuid,
    user_id         uuid,
    content         text,
    attachments     set<frozen<media>>,
    metadata        map<text, text>,
    reaction_counts map<bigint, bigint>,
    created_at      timestamp,
    updated_at      timestamp,
    updated         boolean,
    PRIMARY KEY ( (user_id), chat_group_id)
) WITH CLUSTERING ORDER BY (chat_group_id ASC) AND CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };

CREATE MATERIALIZED VIEW IF NOT EXISTS chat_message_drafts_by_id
AS
SELECT *
FROM chat_message_drafts
WHERE id IS NOT NULL
  AND chat_group_id IS NOT NULL
  AND user_id IS NOT NULL
PRIMARY KEY ( id, user_id, chat_group_id );

CREATE INDEX IF NOT EXISTS chat_message_draft_id_idx ON chat_message_drafts (id);

CREATE TABLE IF NOT EXISTS chat_messages_reply_count
(
    message_id  uuid,
    reply_count counter,
    PRIMARY KEY (message_id)
) WITH CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };

CREATE TABLE IF NOT EXISTS chat_message_replies
(
    id              uuid,
    chat_group_id   uuid,
    reply_to_id     uuid,
    user_id         uuid,
    content         text,
    attachments     set<frozen<media>>,
    metadata        map<text, text>,
    reaction_counts map<bigint, bigint>,
    created_at      timestamp,
    updated_at      timestamp,
    replies_count   bigint static,
    updated         boolean,
    PRIMARY KEY ( (reply_to_id), created_at)
) WITH CLUSTERING ORDER BY (created_at DESC) AND CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };

CREATE INDEX IF NOT EXISTS chat_message_replies_id_idx ON chat_message_replies (id);

CREATE TABLE IF NOT EXISTS friend_invitations
(
    id         uuid,
    inviter_id uuid,
    invitee_id uuid,
    metadata   map<text, text>,
    status     tinyint,
    created_at timestamp,
    updated_at timestamp,
    updated    boolean,
    PRIMARY KEY ( (inviter_id), created_at)
) WITH CLUSTERING ORDER BY (created_at DESC) AND CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };

CREATE INDEX IF NOT EXISTS friend_invitations_id_idx ON friend_invitations (id);
CREATE INDEX IF NOT EXISTS friend_invitations_invitee_id_idx ON friend_invitations (invitee_id);

CREATE TABLE IF NOT EXISTS chat_message_reactions
(
    id              uuid,
    message_id      uuid,
    chat_group_id   uuid,
    user_id         uuid,
    username        text,
    reaction_code   bigint,
    metadata        map<text, text>,
    created_at      timestamp,
    updated_at      timestamp,
    reaction_counts map<bigint, bigint> static,
    updated         boolean,
    PRIMARY KEY ((message_id), created_at, id)
) WITH CLUSTERING ORDER BY (created_at DESC, id ASC) AND CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };

CREATE INDEX IF NOT EXISTS message_reactions_id_idx ON chat_message_reactions (id);

CREATE TABLE IF NOT EXISTS chat_group_members
(
    id              uuid,
    user_id         uuid,
    chat_group_id   uuid,
    username        text,
    created_at      timestamp,
    metadata        map<text, text>,
    members_count   bigint static,
    membership_type tinyint,
    PRIMARY KEY ( (chat_group_id), created_at, user_id )
) WITH CLUSTERING ORDER BY (created_at DESC, user_id ASC) AND CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };

CREATE MATERIALIZED VIEW IF NOT EXISTS chat_group_members_by_user_id AS
SELECT id, user_id, chat_group_id, created_at, metadata, membership_type
FROM chat_group_members
WHERE user_id IS NOT NULL
  AND created_at IS NOT NULL
  AND chat_group_id IS NOT NULL
PRIMARY KEY ( (user_id), created_at, chat_group_id );

// Chat message replies summary info table:
CREATE TABLE IF NOT EXISTS chat_message_replies_summaries
(
    chat_group_id uuid,
    message_id    uuid,
    id            uuid,
    created_at    timestamp,
    updated       boolean,
    updated_at    timestamp,
    total         bigint,
    replier_ids   set<uuid>,
    replier_infos set<frozen<message_replier_info>>,
    PRIMARY KEY ( (chat_group_id), created_at )
) WITH CLUSTERING ORDER BY (created_at DESC)
   AND CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };

CREATE INDEX IF NOT EXISTS chat_message_reply_summaries_message_id_idx on chat_message_replies_summaries (message_id);

CREATE TABLE IF NOT EXISTS chat_group_attachments
(
    chat_group_id     uuid,
    attachment_id     uuid,
    user_id           uuid,
    username          text,
    media_info        media,
    created_at        timestamp,
    attachments_count bigint static,
    updated_at        timestamp,
    PRIMARY KEY ( (chat_group_id), created_at, attachment_id )
) WITH CLUSTERING ORDER BY (created_at DESC, attachment_id ASC)
   AND CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };

CREATE TABLE IF NOT EXISTS chat_group_join_requests
(
    id            uuid,
    user_id       uuid,
    chat_group_id uuid,
    created_at    timestamp,
    status        tinyint,
    updated_at    timestamp,
    PRIMARY KEY ( (chat_group_id), created_at, user_id )
) WITH CLUSTERING ORDER BY (created_at DESC, user_id ASC)
   AND CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };

CREATE TABLE IF NOT EXISTS user_notifications
(
    id         uuid,
    user_id    uuid,
    type       tinyint,
    metadata   map<text, text>,
    summary    text,
    read       boolean,
    created_at timestamp,
    updated_at timestamp,
    PRIMARY KEY ( (user_id), created_at )
) WITH CLUSTERING ORDER BY (created_at DESC)
   AND CACHING = { 'keys': 'ALL', 'rows_per_partition': 100 };

CREATE INDEX IF NOT EXISTS user_notifications_id_idx on user_notifications (id);
