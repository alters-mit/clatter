use std::f64::consts::TAU;
use safer_ffi::ffi_export;
#[cfg(feature = "headers")]
use safer_ffi::headers::Language::CSharp;
use safer_ffi::Vec as SafeVec;

/// No-op to let the C# library check if it can load this library.
#[ffi_export]
pub fn is_ok() {}

#[ffi_export]
pub fn convolve(
    input: &SafeVec<f64>,
    kernel: &SafeVec<f64>,
    output: &mut SafeVec<f64>,
    length: usize,
) {
    let input_length = input.len();
    let kernel_length = kernel.len();
    for (i, o) in (0..length - 1).zip(output.iter_mut()).rev() {
        *o = kernel[if i < input_length {
            0
        } else {
            i - input_length - 1
        }..=if i < kernel_length {
            0
        } else {
            kernel_length - 1
        }]
            .iter()
            .enumerate()
            .map(|(j, k)| input[i - j] * *k)
            .sum();
    }
}

#[ffi_export]
pub fn ir_sinusoid(
    power: f64,
    decay: f64,
    frequency: f64,
    resonance: f64,
    mode_count: usize,
    framerate: f64,
    mode: &mut SafeVec<f64>,
) {
    let pow = 10.0f64.powf(power / 20.0);
    let dcy = -60.0 / (decay * resonance / 1e3) / 20.0;
    let q = frequency * TAU;
    (0..mode_count)
        .map(|t| t as f64 / framerate)
        .zip(mode.iter_mut())
        .for_each(|(t, m)| *m = (t * q).cos() * pow * 10.0f64.powf(t * dcy));
}

#[cfg(feature = "headers")]
pub fn generate_cs() -> ::std::io::Result<()> {
    let builder = safer_ffi::headers::builder().with_language(CSharp);
    if ::std::env::var("HEADERS_TO_STDOUT")
        .ok()
        .map_or(false, |it| it == "1")
    {
        builder.to_writer(::std::io::stdout()).generate()?
    } else {
        builder
            .to_file(&format!("../Clatter/Clatter.Core/NativeBindings.cs"))?
            .generate()?
    }
    Ok(())
}
