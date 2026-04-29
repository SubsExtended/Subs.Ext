-- language_level.lua
-- Put this in %APPDATA%\vlc\lua\extensions\

local selected_level = 3 -- Default: 1=A, 2=B, 3=C, 4=D, 5=E
local subs = {}

function descriptor()
    return {
        title = "My Language Level",
        version = "1.1",
        capabilities = { "menu", "input-listener" }
    }
end

function menu()
    return { "Level A", "Level B", "Level C (Default)", "Level D", "Level E", "---", "Scan for Subtitles" }
end

function trigger_menu(id)
    if id <= 5 then
        selected_level = id
        vlc.osd.message("Filter: Show up to Level " .. string.char(64 + selected_level), 0, "bottom", 2000000)
    elseif id == 7 then
        load_srt_file()
    end
end

-- Converts "00:00:20,000" to milliseconds
function parse_time(time_str)
    local h, m, s, ms = string.match(time_str, "(%d+):(%d+):(%d+),(%d+)")
    return (h*3600000) + (m*60000) + (s*1000) + ms
end

function load_srt_file()
    subs = {} -- Clear old data
    local item = vlc.input.item()
    if not item then return end

    -- Get video path and swap extension to .srt
    local uri = item:uri()
    local srt_path = string.gsub(uri, "^file:///", "") -- Windows path cleanup
    srt_path = string.gsub(srt_path, "%%20", " ")      -- Handle spaces
    srt_path = string.gsub(srt_path, "%.[^%.]+$", ".srt")

    local file = io.open(srt_path, "r")
    if not file then 
        vlc.osd.message("No matching .srt file found!", 0, "bottom", 3000000)
        return 
    end

    local current_sub = {}
    for line in file:lines() do
        -- 1. Look for timecode line
        local start_t, end_t = string.match(line, "(%d+:%d+:%d+,%d+) %-%-> (%d+:%d+:%d+,%d+)")
        if start_t then
            current_sub.start = parse_time(start_t)
            current_sub.stop = parse_time(end_t)
            current_sub.text = ""
        -- 2. Look for rating tag {DIFF:X}
        elseif string.match(line, "{DIFF:([A-E])}") then
            local grade = string.match(line, "{DIFF:([A-E])}")
            current_sub.rating = string.byte(grade) - 64 -- A=1, B=2, etc.
            current_sub.text = string.gsub(line, "{DIFF:[A-E]}", "") -- Clean tag for display
            table.insert(subs, current_sub)
            current_sub = {}
        end
    end
    file:close()
    vlc.osd.message("Loaded " .. #subs .. " rated subtitles.", 0, "bottom", 3000000)
end

-- Runs continuously during playback
function input_changed()
    local input = vlc.object.input()
    if not input then return end
    
    local curr_ms = vlc.var.get(input, "time") / 1000 -- micro to milli
    for _, sub in ipairs(subs) do
        if curr_ms >= sub.start and curr_ms <= sub.stop then
            -- ONLY display if the rating is EQUAL OR HARDER than selected
            if sub.rating <= selected_level then
                vlc.osd.message(sub.text, 0, "bottom", 500000)
            end
            break
        end
    end
end