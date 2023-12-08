"use client";
import moment from "moment";
import React, { DetailedHTMLProps, TimeHTMLAttributes } from "react";

export interface TimeProps extends DetailedHTMLProps<TimeHTMLAttributes<HTMLTimeElement>, HTMLTimeElement> {
   value: string;
   format?: string;
}

const Time = ({ value, format = "HH:mm DD/MM/YYYY", ...props }: TimeProps) => {
   const { className, ...rest } = props;
   return (
      <time className={`text-xs font-light text-default-500 ${className}`} {...rest}>
         {moment(new Date(value)).format(format)}
      </time>
   );
};

export default Time;
