// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2026 Brainbowbash
shader_type canvas_item;

uniform vec2 subdivisions = vec2(10.0, 5.0);

uniform float axis_thickness = 3.0;
uniform float grid_thickness = 1.0;

uniform vec4 background_color : source_color = vec4(0.12, 0.12, 0.12, 1.0);
uniform vec4 grid_color : source_color = vec4(0.25, 0.25, 0.25, 1.0);
uniform vec4 axis_color : source_color = vec4(0.85, 0.85, 0.85, 1.0);

const float pixel_epsilon = 0.01; // "fixes" visual artifacts related to rounding (i hate shader programming)

void fragment() {

	float scaling_factor = 1./SCREEN_PIXEL_SIZE.y/1080.;
	float true_line_thickness = max(ceil(grid_thickness * scaling_factor),1.);
	float true_axis_thickness = max(ceil(axis_thickness * scaling_factor),1.);
	
	vec2 rect_pixels = (1.0 / fwidth(UV));
	
	vec2 pixel_pos = vec2(UV.x, 1.0 - UV.y) * rect_pixels + vec2(pixel_epsilon);
	
	vec2 cell_size = (rect_pixels) / subdivisions;
	vec2 dist_to_cell = mod((pixel_pos+vec2(ceil(true_line_thickness*0.5))),cell_size);

	vec2 thin_lines = step(dist_to_cell,vec2(true_line_thickness));
	float thin_mask = min(thin_lines.x + thin_lines.y,1.);

	vec2 axis_lines = step(pixel_pos,vec2(true_axis_thickness));
	float axis_mask = min(axis_lines.x+axis_lines.y,1.);

	vec4 final_color = mix(background_color, grid_color, thin_mask);
	COLOR = mix(final_color, axis_color, axis_mask);
}