-- MyLanguageLevel.lua
-- VLC Lua extension for filtering subtitles by {DIFF:X} tags

function descriptor()
    return {
        title = "My Language Level",
        version = "1.0",
        author = "Alexey",
        description = "Filter subtitles by difficulty tags {DIFF:X}",
        shortdesc = "Language Level Filter",
        capabilities = {}
    }
end

local dlg = nil
local current_level = "C"

local levels = { "A", "B", "C", "D", "E" }

function activate()
    show_dialog()
end

function deactivate()
    if dlg then dlg:delete() end
end

function close()
    deactivate()
end

function show_dialog()
    dlg = vlc.dialog("My Language Level")

    dlg:add_label("Select your level:", 1, 1, 1, 1)

    local col = 1
    for _, lvl in ipairs(levels) do
        dlg:add_button(lvl, function() set_level(lvl) end, col, 2, 1, 1)
        col = col + 1
    end

    dlg:add_button("Apply to current subtitles", apply_filter, 1, 3, 3, 1)
end

function set_level(lvl)
    current_level = lvl
    vlc.msg.info("MyLanguageLevel: level set to " .. lvl)
end

-- Difficulty mapping
local function allowed_levels_for(level)
    if level == "A" then return { "A" }
    elseif level == "B" then return { "A", "B" }
    elseif level == "C" then return { "A", "B" }
    elseif level == "D" then return { "A", "B", "C" }
    elseif level == "E" then return { "A", "B", "C", "D" }
    end
    return { "A" }
end

local function has_allowed_diff(line, allowed)
    for _, lvl in ipairs(allowed) do
        if string.find(line, "{DIFF:" .. lvl .. "}", 1, true) then
            return true
        end
    end
    return false
end

local function strip_diff_tags(line)
    return string.gsub(line, "{DIFF:[ABCDE]}", "")
end

function apply_filter()
    local item = vlc.input.item()
    if not item then
        vlc.msg.err("MyLanguageLevel: no input item")
        return
    end

    -- Try to detect subtitle file
    local metas = item:metas()
    local subfile = metas["subfile"] or metas["Subtitles"]

    if not subfile then
        vlc.msg.err("MyLanguageLevel: no subtitle file detected")
        return
    end

    vlc.msg.info("MyLanguageLevel: using subtitle file " .. subfile)

    local allowed = allowed_levels_for(current_level)

    -- Read original SRT
    local f = io.open(subfile, "r")
    if not f then
        vlc.msg.err("MyLanguageLevel: cannot open subtitle file")
        return
    end
    local content = f:read("*all")
    f:close()

    -- Parse SRT blocks
    local blocks = {}
    local current = {}

    for line in string.gmatch(content, "([^\r\n]*)\r?\n") do
        if line == "" and #current > 0 then
            table.insert(blocks, current)
            current = {}
        else
            table.insert(current, line)
        end
    end
    if #current > 0 then table.insert(blocks, current) end

    -- Filter blocks
    local filtered = {}
    for _, block in ipairs(blocks) do
        local keep = false
        for i = 3, #block do
            if has_allowed_diff(block[i], allowed) then
                keep = true
                break
            end
        end

        if keep then
            local new_block = {}
            for i, line in ipairs(block) do
                if i >= 3 then
                    line = strip_diff_tags(line)
                end
                table.insert(new_block, line)
            end
            table.insert(filtered, new_block)
        end
    end

    -- Rebuild SRT
    local out_lines = {}
    local idx = 1
    for _, block in ipairs(filtered) do
        table.insert(out_lines, tostring(idx))
        table.insert(out_lines, block[2])
        for i = 3, #block do
            table.insert(out_lines, block[i])
        end
        table.insert(out_lines, "")
        idx = idx + 1
    end

    -- Write temp file
    local tmp = os.tmpname() .. ".srt"
    local out = io.open(tmp, "w")
    out:write(table.concat(out_lines, "\n"))
    out:close()

    vlc.msg.info("MyLanguageLevel: loading filtered subtitles " .. tmp)
    vlc.input.add_subtitle(tmp, true)
end
