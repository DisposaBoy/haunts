uniform sampler2D tex;
varying vec3 pos;

vec3 mod289(vec3 x) {
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec2 mod289(vec2 x) {
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec3 permute(vec3 x) {
  return mod289(((x*34.0)+1.0)*x);
}

float snoise(vec2 v) {
  const vec4 C = vec4(0.211324865405187,  // (3.0-sqrt(3.0))/6.0
                      0.366025403784439,  // 0.5*(sqrt(3.0)-1.0)
                     -0.577350269189626,  // -1.0 + 2.0 * C.x
                      0.024390243902439); // 1.0 / 41.0
// First corner
  vec2 i  = floor(v + dot(v, C.yy) );
  vec2 x0 = v -   i + dot(i, C.xx);

// Other corners
  vec2 i1;
  //i1.x = step( x0.y, x0.x ); // x0.x > x0.y ? 1.0 : 0.0
  //i1.y = 1.0 - i1.x;
  i1 = (x0.x > x0.y) ? vec2(1.0, 0.0) : vec2(0.0, 1.0);
  // x0 = x0 - 0.0 + 0.0 * C.xx ;
  // x1 = x0 - i1 + 1.0 * C.xx ;
  // x2 = x0 - 1.0 + 2.0 * C.xx ;
  vec4 x12 = x0.xyxy + C.xxzz;
  x12.xy -= i1;

// Permutations
  i = mod289(i); // Avoid truncation effects in permutation
  vec3 p = permute( permute( i.y + vec3(0.0, i1.y, 1.0 ))
    + i.x + vec3(0.0, i1.x, 1.0 ));

  vec3 m = max(0.5 - vec3(dot(x0,x0), dot(x12.xy,x12.xy), dot(x12.zw,x12.zw)), 0.0);
  m = m*m ;
  m = m*m ;

// Gradients: 41 points uniformly over a line, mapped onto a diamond.
// The ring size 17*17 = 289 is close to a multiple of 41 (41*7 = 287)

  vec3 x = 2.0 * fract(p * C.www) - 1.0;
  vec3 h = abs(x) - 0.5;
  vec3 ox = floor(x + 0.5);
  vec3 a0 = x - ox;

// Normalise gradients implicitly by scaling m
// Approximation of: m *= inversesqrt( a0*a0 + h*h );
  m *= 1.79284291400159 - 0.85373472095314 * ( a0*a0 + h*h );

// Compute final noise value at P
  vec3 g;
  g.x  = a0.x  * x0.x  + h.x  * x0.y;
  g.yz = a0.yz * x12.xz + h.yz * x12.yw;
  return 130.0 * dot(m, g);
}

uniform float num_rows;
uniform float noise_rate;
uniform float num_steps;

float eval(vec2 pos, vec2 tex_coord) {
  float grey = texture2D(tex, tex_coord).r;
  float col = floor(pos.x);
  float fudge = snoise(vec2(col, col)) / 9.0;
  float row = floor(pos.y * num_rows / 10.0);
  float n = snoise(vec2(row, pos.x * noise_rate));
  n = n * 0.5 + 0.5;
  n *= grey;
//  n = pow(n, 1.5) * grey;
  n *= num_steps + 0.99;
  n = floor(n);
  n /= num_steps;
//  n = pow(floor((num_steps + 0.99) * n) / num_steps, 0.7);
  return n;
}
uniform int foo;
uniform int range;
const float inc = 0.75;

void main() {
  vec2 pdx = dFdx(pos.xy) / 2.0;
  vec2 pdy = dFdy(pos.xy) / 2.0;
  vec2 tdx = dFdx(gl_TexCoord[0].xy) / 2.0;
  vec2 tdy = dFdy(gl_TexCoord[0].xy) / 2.0;
  float n = 0.0;
  if (foo == 0) {
  float sum = 0.0;

  vec2 tt = gl_TexCoord[0].xy; // + tdy * float(i)*inc;
  vec2 pt = pos.xy;
  float e = eval(pt, tt);
  float tot = 1.0;
  sum += tot;
  n += tot * e;
  float center = e;

  tt = gl_TexCoord[0].xy + tdy * 1.0 * inc;
  pt = pos.xy + pdy * 1.0 * inc;
  e = eval(pt, tt);
  tot = 0.75;
  sum += tot;
  n += tot * e;

  tt = gl_TexCoord[0].xy + tdy * -1.0 * inc;
  pt = pos.xy + pdy * -1.0 * inc;
  e = eval(pt, tt);
  tot = 0.75;
  sum += tot;
  n += tot * e;

//  if (sum * center + 0.001 > n && sum * center - 0.001 < n) {
//    n /= sum;
//    gl_FragColor = vec4(n, n, n, 1.0);
//    return;
//  }

  tt = gl_TexCoord[0].xy + tdx * 1.0 * inc;
  pt = pos.xy + pdx * 1.0 * inc;
  e = eval(pt, tt);
  tot = 0.75;
  sum += tot;
  n += tot * e;

  tt = gl_TexCoord[0].xy + tdx * -1.0 * inc;
  pt = pos.xy + pdx * -1.0 * inc;
  e = eval(pt, tt);
  tot = 0.75;
  sum += tot;
  n += tot * e;

  tt = gl_TexCoord[0].xy + tdx * 1.0 + tdy * 1.0 * inc;
  pt = pos.xy + pdx * 1.0 + pdy * 1.0 * inc;
  e = eval(pt, tt);
  tot = 0.5;
  sum += tot;
  n += tot * e;

  tt = gl_TexCoord[0].xy + tdx * 1.0 + tdy * -1.0 * inc;
  pt = pos.xy + pdx * 1.0 + pdy * -1.0 * inc;
  e = eval(pt, tt);
  tot = 0.5;
  sum += tot;
  n += tot * e;

  tt = gl_TexCoord[0].xy + tdx * -1.0 * inc + tdy * 1.0 * inc;
  pt = pos.xy + pdx * -1.0 * inc + tdy * 1.0 * inc;
  e = eval(pt, tt);
  tot = 0.5;
  sum += tot;
  n += tot * e;

  tt = gl_TexCoord[0].xy + tdx * -1.0 * inc + tdy * -1.0 * inc;
  pt = pos.xy + pdx * -1.0 * inc + pdy * -1.0 * inc;
  e = eval(pt, tt);
  tot = 0.5;
  sum += tot;
  n += tot * e;


  n /= sum;
  } else {
    n = eval(gl_TexCoord[0].xy, gl_TexCoord[0].xy);
  }
  gl_FragColor = vec4(n, n, n, 1.0);
}
