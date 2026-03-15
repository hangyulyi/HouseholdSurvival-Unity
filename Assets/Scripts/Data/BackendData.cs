using System;

// ============================================================
//  COUNTRY DATA
// ============================================================

[Serializable]
public class CountriesResponse
{
    public CountryData[] countries;
}

[Serializable]
public class CountryDetailResponse
{
    public CountryData country;
    public CountryEventData[] events;
}

[Serializable]
public class CountryData
{
    public string country_code;
    public string country_name;
    public string flag_emoji;
    public int starting_money;
    public int starting_health;
    public int starting_stress;
    public int starting_happiness;
    public int starting_debt;
    public float healthcare_cost_mult;
    public float education_access_mult;
    public float safety_net_mult;
    public int threshold_stabilized;
    public int threshold_survival;
    public int threshold_poverty;
    public string visual_setting;
    public string ambient_sound;
    public string difficulty_label;
    public string intro_text;
}

[Serializable]
public class CountryEventData
{
    public int event_id;
    public string country_code;
    public int event_phase;
    public string event_title;
    public string event_description;
    public string choice_a_text;
    public int choice_a_economic;
    public int choice_a_social;
    public int choice_a_health;
    public string choice_b_text;
    public int choice_b_economic;
    public int choice_b_social;
    public int choice_b_health;
    public string choice_c_text;   // may be null / empty
    public int choice_c_economic;
    public int choice_c_social;
    public int choice_c_health;
}

[Serializable]
public class CountryEventWrapper
{
    public CountryEventData @event;  // "event" is a reserved word — use verbatim
}

// ============================================================
//  SESSION
// ============================================================

[Serializable]
public class SessionStartResponse
{
    public SessionData session;
    public CountryData country_config;  // merged DB + World Bank values
}

[Serializable]
public class SessionData
{
    public int session_id;
    public int user_id;
    public string country_code;
    public string character_name;
    public string final_ending;
    public int total_score;
    public string started_at;
    public string completed_at;
}

// ============================================================
//  SCENARIOS & DECISIONS
// ============================================================

[Serializable]
public class ScenarioResponse
{
    public ScenarioData scenario;
    public DecisionData[] decisions;
    public CountryEventData country_event;  // may be null
}

[Serializable]
public class ScenarioData
{
    public int scenario_id;
    public int phase_number;
    public string title;
    public string description;
    public string sdg_goal;
    public string difficulty;
}

[Serializable]
public class DecisionData
{
    public int decision_id;
    public int scenario_id;
    public string choice_text;
    public bool is_minigame_trigger;
    public int impact_score;
    public int economic_score;
    public int social_score;
    public int health_score;
    public int environmental_score;
}

// ============================================================
//  DECISION SUBMIT RESPONSE
// ============================================================

[Serializable]
public class DecisionSubmitResponse
{
    public string message;
    public DecisionData chosen_decision;
    public AdjustedScores adjusted_scores;
    public ProgressEntry updated_progress;
    public string final_outcome;   // non-null only on phase 7
}

[Serializable]
public class AdjustedScores
{
    public int impact_score;
    public int economic_score;
    public int social_score;
    public int health_score;
    public int environmental_score;
}

[Serializable]
public class ProgressEntry
{
    public int progress_id;
    public int user_id;
    public int scenario_id;
    public int decision_id;
    public int score;
    public bool completed;
    public string last_played;
}

// ============================================================
//  EVENT DECISION RESPONSE
// ============================================================

[Serializable]
public class EventDecisionResponse
{
    public string message;
    public int event_id;
    public string chosen_choice;
    public string chosen_text;
    public EventDelta delta;
}

[Serializable]
public class EventDelta
{
    public int economic;
    public int social;
    public int health;
    public int impact;
}

// ============================================================
//  PROGRESS & LEADERBOARD
// ============================================================

[Serializable]
public class ProgressResponse
{
    public ProgressEntry[] progress;
}

[Serializable]
public class LeaderboardResponse
{
    public LeaderboardEntry[] leaderboard;
}

[Serializable]
public class LeaderboardEntry
{
    public int leaderboard_id;
    public string username;
    public string email;
    public string country_code;
    public int total_score;
}
