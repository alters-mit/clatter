use lazy_static::lazy_static;
use safer_ffi::ffi_export;
#[cfg(feature = "headers")]
use safer_ffi::headers::Language::CSharp;
use std::f64::consts::{PI, TAU};

const SCRAPE_LINEAR_SPACE_LEN: usize = 4410;
const SCRAPE_LINEAR_SPACE_STEP: f64 = 1.0 / (SCRAPE_LINEAR_SPACE_LEN - 1) as f64;
lazy_static! {
    static ref SCRAPE_LINEAR_SPACE: Vec<f64> = (0usize..SCRAPE_LINEAR_SPACE_LEN)
        .map(|i| i as f64 * SCRAPE_LINEAR_SPACE_STEP)
        .collect();
}

type SafeVec = safer_ffi::Vec<f64>;

#[ffi_export]
pub struct MedianFilter {
    pub buffer: SafeVec,
    pub offset_buffer_1: SafeVec,
    pub offset_buffer_2: SafeVec,
    pub offset_buffer_3: SafeVec,
    pub offset_buffer_4: SafeVec,
    pub offset: usize,
    pub full: bool,
}

impl MedianFilter {
    pub(crate) fn process(&mut self, sample: f64) -> f64 {
        self.offset = if self.offset == 0 { 4 } else { self.offset - 1 };
        self.buffer[self.offset] = sample;
        self.full |= self.offset == 0;
        if self.full {
            Self::median_in_place(&mut self.buffer)
        } else {
            let length = 5 - self.offset;
            // Get the offset buffer.
            let offset_buffer = match length {
                1 => &mut self.offset_buffer_1,
                2 => &mut self.offset_buffer_2,
                3 => &mut self.offset_buffer_3,
                4 => &mut self.offset_buffer_4,
                other => unreachable!("Median filter offset buffer length wants to be {}", other),
            };
            // Copy to the offset buffer.
            offset_buffer[0..length].copy_from_slice(&self.buffer[0..length]);
            Self::median_in_place(offset_buffer)
        }
    }

    fn median_in_place(data: &mut SafeVec) -> f64 {
        let k = (data.len() / 2) as i32;
        if data.len() % 2 != 0 {
            Self::select_in_place(data, k)
        } else {
            (Self::select_in_place(data, k - 1) + Self::select_in_place(data, k)) / 2.0
        }
    }

    fn select_in_place(data: &mut SafeVec, rank: i32) -> f64 {
        if rank <= 0 {
            *data.iter().min_by(|&a, &b| a.total_cmp(b)).unwrap()
        } else if rank as usize >= data.len() - 1 {
            *data.iter().max_by(|&a, &b| a.total_cmp(b)).unwrap()
        } else {
            let rank = rank as usize;
            let mut low = 0;
            let mut high = data.len() - 1;
            for _ in 0..100 {
                let low1 = low + 1;
                if high <= low1 {
                    if high == low1 && data[high] < data[low] {
                        data.swap(low, high);
                    }
                    return data[rank];
                }
                data.swap((low + high) >> 1, low1);
                if data[low] > data[high] {
                    data.swap(low, high);
                }
                if data[low] > data[low1] {
                    data.swap(low, low1);
                }
                let pivot = data[low1];
                let mut begin = low1;
                let mut end = high;

                // Unclear how many times we need to iterate?
                for _ in 0usize..100 {
                    begin = data[begin..].iter().enumerate().find(|(_, &v)| v < pivot).unwrap().0;
                    end = data[0..end].iter().rev().enumerate().find(|(_, &v)| v > pivot).unwrap().0;
                    if end < begin {
                        break;
                    }
                    data.swap(begin, end);
                }

                data[low1] = data[end];
                data[end] = pivot;
                if end >= rank {
                    high = end - 1;
                }
                if end <= rank {
                    low = begin;
                }
            }
            0.0
        }
    }
}

/// No-op to let the C# library check if it can load this library.
#[ffi_export]
pub fn is_ok() {}

/// Convolve the input by the kernel.
///
/// Source: https://stackoverflow.com/a/7239016
/// This code is a more optimized version of the source.
///
/// We're not using an fft convolve because it's actually faster to convolve in-place without ndarray.
///
/// - `input` The input array.
/// - `kernel` A convolution kernel.
/// - `length` The length of the convolved array.
/// - `output` The output array.
#[ffi_export]
pub fn convolve(input: &SafeVec, kernel: &SafeVec, length: usize, output: &mut SafeVec) {
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

/// Synthesize a sinusoid from mode data.
///
/// - `power` The mode onset powers in dB.
/// - `decay` The mode decay time i.e. the time in ms it takes for this mode to decay 60dB from its onset power.
/// - `frequency` The mode frequency in Hz.
/// - `resonance` The object's resonance value.
/// - `mode_count` The actual length of the sinusoid.
/// - `framerate` The audio framerate.
#[ffi_export]
pub fn mode_sinusoid(
    power: f64,
    decay: f64,
    frequency: f64,
    resonance: f64,
    mode_count: usize,
    framerate: f64,
    mode: &mut SafeVec,
) {
    let pow = 10.0f64.powf(power / 20.0);
    let dcy = -60.0 / (decay * resonance / 1e3) / 20.0;
    let q = frequency * TAU;
    (0..mode_count)
        .map(|t| t as f64 / framerate)
        .zip(mode.iter_mut())
        .for_each(|(t, m)| *m = (t * q).cos() * pow * 10.0f64.powf(t * dcy));
}

/// Create a sine wave for impact audio.
///
/// - `length` The array will be filled up to this length.
/// - `arr` The array that will be filled.
#[ffi_export]
pub fn impact_frequencies(length: usize, arr: &mut SafeVec) {
    let step = PI / (length - 1) as f64;
    arr[0..length]
        .iter_mut()
        .enumerate()
        .for_each(|(i, v)| *v = (i as f64 * step).sin());
}

pub fn get_scrape(
    primary_mass: f64,
    scrape_speed: f64,
    max_speed: f64,
    roughness_ratio: f64,
    simulation_amp: f64,
    scrape_amp: f64,
    scrape_index: &mut usize,
    num_points: usize,
    dsdx: &SafeVec,
    d2sdx2: &SafeVec,
    linear_space: &mut SafeVec,
    force: &mut SafeVec,
    impulse_response: &SafeVec,
    median_filter: &mut MedianFilter,
    samples: &mut SafeVec,
) {
    // Define the linear space.
    let step = 1.0 / (num_points - 1) as f64;
    linear_space[0..num_points]
        .iter_mut()
        .enumerate()
        .for_each(|(i, v)| *v = i as f64 * step);

    // Define and reset the indices.
    let mut final_index = *scrape_index + num_points;
    if final_index > dsdx.len() {
        *scrape_index = 0;
        final_index = num_points;
    }

    // Calculate the force by adding the horizontal force and the vertical force.
    // The horizontal force is the interpolation of the dsdx array multiplied by a factor.
    // The vertical force is a median filter sample of tanh of (the interpolation of the d2sdx2 array multiplied by a factor).
    let mut horizontal_interpolation_index = 0;
    let mut vertical_interpolation_index = 0;
    let vertical = 0.5 * (scrape_speed / max_speed).powf(2.0);
    let horizontal = 0.05 * (scrape_speed / max_speed);
    let curve_mass = 10.0 * primary_mass;
    let lower_dsdx = dsdx[*scrape_index];
    let upper_dsdx = dsdx[final_index];
    let lower_d2sdx2 = d2sdx2[*scrape_index];
    let upper_d2sdx2 = d2sdx2[final_index];
    for (s, f) in SCRAPE_LINEAR_SPACE.iter().zip(force.iter_mut()) {
        *f = horizontal
            * interpolate1d(
                *s,
                linear_space,
                dsdx,
                lower_dsdx,
                upper_dsdx,
                *scrape_index,
                &mut horizontal_interpolation_index,
                num_points,
            )
            + vertical
                * median_filter.process(
                    (interpolate1d(
                        *f,
                        linear_space,
                        d2sdx2,
                        lower_d2sdx2,
                        upper_d2sdx2,
                        *scrape_index,
                        &mut vertical_interpolation_index,
                        num_points,
                    ) / curve_mass)
                        .tanh(),
                );
    }

    // Convolve.
    convolve(impulse_response, force, SCRAPE_LINEAR_SPACE_LEN, samples);

    // Apply roughness and amp.
    let a = roughness_ratio * simulation_amp * scrape_amp;
    samples.iter_mut().for_each(|v| *v *= a);

    // Update the scrape index.
    *scrape_index = final_index;
}

fn interpolate1d(
    v: f64,
    x: &SafeVec,
    y: &SafeVec,
    lower: f64,
    upper: f64,
    y_index_offset: usize,
    start_x: &mut usize,
    end_x: usize,
) -> f64 {
    for (i, ix) in x[*start_x..end_x].iter().enumerate() {
        if v < *ix {
            *start_x = i + 1;
            if i == 0 {
                return lower;
            }
            let s = i - 1;
            let x0 = x[s];
            let y0 = y[s + y_index_offset];
            return y0 + (y[i + y_index_offset] - y0) * (v - x0) / (*ix / x0);
        }
    }
    *start_x = 0;
    upper
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
