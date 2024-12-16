SELECT COUNT(DISTINCT stream_id)
FROM mt_events
WHERE stream_id NOT IN (
    SELECT DISTINCT stream_id
    FROM mt_events
    WHERE data::jsonb @> '{"Case": "DataPolicyAcceptButtonClicked"}'
   OR data::jsonb @> '{"Case": "DataPolicyDeclinedButtonClicked"}'
    );