﻿#light

namespace BitImage
module StructuringElementUtils

open Image
open Utility
open StructuringElement

let EmptyStructuringElement size =
  let center = size / 2
  let pattern = Array2D.create size size (System.Nullable<bool>(false))
  new StructuringElement<System.Nullable<bool>>(center, center, pattern)

let SquareStructuringElement size =
  let center = size / 2
  let pattern = Array2D.create size size (System.Nullable<bool>(true))
  new StructuringElement<System.Nullable<bool>>(center, center, pattern)
  
let SpiralStructuringElement size =
  let center = size / 2 in
  let pqs = [for i in 0 .. size * size  - 1 -> spiral i] in
  let arr = Array2D.zeroCreate size size in
  let setElem i elem =
    let (p, q) = elem
    let r = center + p in
    let s = center + q in
    Array2D.set arr r s (1 <<< i)
  List.iteri setElem pqs
  new StructuringElement<int>(center, center, arr)

/// Rotate a StructuringElement by 90° anticlockwise. NB. Does not take origin into account
let rotate (s:StructuringElement<'a>) =
  let arr = Array2D.init s.SizeJ s.SizeI (fun i j -> s.[s.Right - j, s.Bottom + i])
  new StructuringElement<'a>(s.OriginJ, s.OriginI, arr)
    
let fourRotatedElements (element:StructuringElement<'a>) =
  let rec prependRotated i elements =
    match i with
    | 0 -> elements
    | i -> prependRotated (i - 1) ((elements.Head |> rotate) :: elements)
  prependRotated 3 [element]

let twoRotatedElements (element:StructuringElement<'a>) =
  (element |> rotate) :: [element]

let StructuringElementFromStrings strs =
  let elementArray = strs |> stringListToNullableBools |> array2FromListOfLists
  new StructuringElement<System.Nullable<bool>>(1, 1, elementArray)

/// Given an integer structuring element, sum the score for each associated
/// structuring element pixel
let neighbourScore (sElem:StructuringElement<int>) (image:IImage<bool>) i j =
  let rec testPixels accum coords =
    if not (List.isEmpty coords) then
      let (p, q) = coords.Head in
      let s = i + p in 
      let t = j + q in
      if image.IsInRange(s, t) then
        match image.[s, t] with
        | false -> testPixels accum coords.Tail
        | true  -> testPixels (accum + sElem.[p, q]) coords.Tail
      else
        testPixels accum coords.Tail
    else
      accum
  sElem.CoordList |> testPixels 0


