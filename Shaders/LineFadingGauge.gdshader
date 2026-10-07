// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2026 Brainbowbash
shader_type canvas_item;

uniform vec2 limits;
uniform vec2 limit_sharpness;
void fragment() {
	float line_bounds = smoothstep(0.0,1.0,(UV.x-limits.x)*limit_sharpness.x)*smoothstep(0.0,1.0,(limits.y-UV.x)*limit_sharpness.y);
	
	float line_smoothing = smoothstep(0.7, 1.0, abs(UV.y - 0.5) * 2.0);
	float alpha = line_bounds - line_smoothing;
	COLOR.a *= alpha;
}