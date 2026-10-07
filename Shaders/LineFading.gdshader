shader_type canvas_item;

void fragment() {
    float dist = abs(UV.y - 0.5) * 2.0; 
    float alpha = 1.0 - smoothstep(0.7, 1.0, dist);
    COLOR.a *= alpha;
}