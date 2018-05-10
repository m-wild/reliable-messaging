/* -- rollback
DROP TABLE Backend.Processed;
DROP SCHEMA Backend;

DROP TABLE Communication;
DROP TABLE Event;
DROP TABLE Request;
DROP EXTENSION citext;
DROP EXTENSION pgcrypto;
*/

CREATE EXTENSION IF NOT EXISTS pgcrypto;
CREATE EXTENSION IF NOT EXISTS citext;

CREATE TABLE IF NOT EXISTS Request (
    RequestId UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    CreatedAt TIMESTAMP NOT NULL DEFAULT current_timestamp
);

CREATE TABLE IF NOT EXISTS Event (
    EventId UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    RequestId UUID NOT NULL REFERENCES Request (RequestId),
    CreatedAt TIMESTAMP NOT NULL DEFAULT current_timestamp,
    SentAt TIMESTAMP NULL,
    Key CITEXT NOT NULL,
    Payload JSON NULL
);

CREATE TABLE IF NOT EXISTS Communication (
    CommunicationId UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    CustomerId UUID NOT NULL,
    TemplateKey CITEXT NOT NULL,
    Payload CITEXT NULL
);


-- SELECT * FROM Request;
-- SELECT * FROM Event;
-- SELECT * FROM Communication;

CREATE SCHEMA Backend;

CREATE TABLE Backend.Processed (
    EventId UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    CreatedAt TIMESTAMP NOT NULL DEFAULT current_timestamp
);

-- SELECT * FROM Backend.Processed;


