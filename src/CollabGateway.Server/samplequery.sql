WITH streams_with_geoip AS (
    SELECT DISTINCT stream_id
    FROM mt_events
    WHERE data::jsonb @> '{"Case": "GeoIPInformation"}'
),
streams_with_datapolicy AS (
    SELECT DISTINCT stream_id
    FROM mt_events
    WHERE data::jsonb @> '{"Case": "DataPolicyAcceptButtonClicked"}'
       OR data::jsonb @> '{"Case": "DataPolicyDeclinedButtonClicked"}'
    AND is_archived = false
)
SELECT COUNT(DISTINCT stream_id) AS stream_count
FROM streams_with_geoip
WHERE stream_id NOT IN (SELECT stream_id FROM streams_with_datapolicy);

/* The following query is a bit more complex, but still straightforward.
   It counts the number of active streams that have GeoIP information, but do
   not have a Data Policy decision (human action). It also groups the results
   by the ASN (Autonomous System Number), ASN 'type' and orders them. */
WITH streams_with_datapolicy AS (
    SELECT DISTINCT stream_id
    FROM mt_events
    WHERE data::jsonb @> '{"Case": "DataPolicyAcceptButtonClicked"}'
       OR data::jsonb @> '{"Case": "DataPolicyDeclinedButtonClicked"}'
),
active_streams_without_datapolicy AS (
    SELECT DISTINCT stream_id
    FROM mt_events
    WHERE stream_id NOT IN (SELECT stream_id FROM streams_with_datapolicy)
)
SELECT data->'Fields'->0->'UserGeoInfo'->'as'->>'asn' AS ASnumber,
       data->'Fields'->0->'UserGeoInfo'->>'isp' AS ISP,
       data->'Fields'->0->'UserGeoInfo'->'as'->>'type' AS ASNtype,
       COUNT(DISTINCT stream_id) AS stream_count
FROM mt_events
WHERE stream_id IN (SELECT stream_id FROM active_streams_without_datapolicy)
  AND data->'Fields'->0->'UserGeoInfo'->'as'->>'asn' IS NOT NULL
GROUP BY ASnumber, ISP, ASNtype
ORDER BY stream_count DESC;