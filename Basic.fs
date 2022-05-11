#version 430 core

layout(location=0) out vec4 out_color;
 
in vec4 vertexColor; // Now interpolated across face
in vec4 interPos;
in vec3 interNormal;
in vec2 interUV;
in vec3 interTangent;

struct PointLight{ vec4 pos; vec4 color;};

uniform PointLight light;
uniform float metallic;
uniform float roughness;

uniform sampler2D diffuseTexture;
uniform sampler2D normalTexture;
const float PI = 3.14159265359;

// HW7
vec3 getFresnelAtAngleZero(vec3 albedo, float metallic) {

    vec3 F0 = vec3(0.04);

    F0 = mix(F0, albedo, metallic);

    return F0;
}
vec3 getFresnel(vec3 F0, vec3 L, vec3 H) {

    float maxOf0 = max(0, dot(L, H));
   return F0 + (1 - F0) * pow(( 1 - maxOf0 ), 5);

}
float getNDF(vec3 H, vec3 N, float roughness) {
	float a = pow(roughness,2);
	float aTo2 = pow(a,2);
	float nAndH = dot(N,H);
	float NHpow = pow(nAndH,2);
	float nextstep = NHpow * (aTo2 - 1);
	nextstep += 1;
	return aTo2 / (PI * pow(nextstep,2));
}
float getSchlickGeo(vec3 B, vec3 N, float roughness) { 

    float k = pow((roughness + 1), 2) / 8;
	return (dot(N,B) / (dot(N,B) * (1 - k ) + k));
}
float getGF(vec3 L, vec3 V, vec3 N, float roughness) {
    float GL = getSchlickGeo(L, N, roughness);

    float GV = getSchlickGeo(V, N, roughness);

    return GL * GV;
}

void main()
{	
	// Just output interpolated color
	//out_color = vertexColor;
	vec3 texColor = vec3(texture(diffuseTexture, interUV));
	//Assign06
	vec3 N = normalize(interNormal);
	vec3 L = vec3(normalize(light.pos - interPos ));

	vec3 T = normalize(interTangent);
	T = normalize(T - dot(T,N) * N);
	vec3 B = normalize(cross (N, T));
	vec3 texN = vec3(texture(normalTexture, interUV));
	texN.x = texN.x * 2.0f - 1.0f;
	texN.y = texN.y * 2.0f - 1.0f;
	texN = normalize(texN);
	mat3 toView = mat3(T, B, N);
	N = normalize(toView * texN);

	float diffuseCoef = max(0, dot(L,N));
	vec3 diffColor  = vec3(diffuseCoef * texColor * vec3(light.color));
	//out_color = vec4(diffColor, 1.0);
	// HW7
	vec3 V = normalize(-vec3(interPos));
	vec3 F0 = getFresnelAtAngleZero(vec3(texColor), metallic);
	vec3 H = normalize(L + V);
	vec3 F = getFresnel(F0, L, H);
	vec3 kS = F;
	vec3 kD = 1.0 - kS;
    kD = kD * (1.0 - metallic);
    kD= vec3(texColor);
    kD /= PI;
	float NDF = getNDF(H, N, roughness);
    float G = getGF(L, V, N, roughness);
    kS = kS * NDF * G;
    kS /= (4.0 * max(0, dot(N, L)) * max(0, dot(N,V)) + 0.0001);
	vec3 finalColor = (kD + kS) * vec3(light.color) * max(0, dot(N,L));
	out_color = vec4(finalColor, 1.0);
}
