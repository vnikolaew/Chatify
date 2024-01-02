"use client";
import React, { PropsWithChildren } from "react";
import { __IS_DEV__ } from "@web/utils";

export interface DevOnlyProps extends PropsWithChildren {

}

const DevOnly = ({ children }: DevOnlyProps) => {
   return __IS_DEV__() && children;
};

export default DevOnly;
