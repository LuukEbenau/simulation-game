shader_type spatial;

uniform float alpha : hint_range(0, 1) = 1.0;
uniform bool use_grayscale = false;

void fragment() {
    vec4 texture_color = texture(TEXTURE, UV);

    if (use_grayscale) {
        float gray = dot(texture_color.rgb, vec3(0.299, 0.587, 0.114));
        ALBEDO = vec3(gray);
    } else {
        ALBEDO = texture_color.rgb;
    }

    ALPHA = alpha * texture_color.a;
}
