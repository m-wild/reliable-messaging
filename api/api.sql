/* -- rollback

DROP TABLE communication;
DROP TABLE event;
DROP TABLE request;
DROP EXTENSION citext;
DROP EXTENSION pgcrypto;
*/

CREATE EXTENSION IF NOT EXISTS pgcrypto;
CREATE EXTENSION IF NOT EXISTS citext;

CREATE TABLE IF NOT EXISTS request (
    request_id UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMP NOT NULL DEFAULT current_timestamp
);

CREATE TABLE IF NOT EXISTS event (
    event_id UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    created_at TIMESTAMP NOT NULL DEFAULT current_timestamp,
    payload JSON NULL
);

CREATE TABLE IF NOT EXISTS communication (
    communication_id UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_id UUID NOT NULL,
    template_key CITEXT NOT NULL,
    payload CITEXT NULL
);
