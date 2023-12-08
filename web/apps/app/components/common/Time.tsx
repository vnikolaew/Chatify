"use client";
import moment, { Moment } from "moment";
import React, { DetailedHTMLProps, TimeHTMLAttributes } from "react";

export interface TimeProps extends DetailedHTMLProps<TimeHTMLAttributes<HTMLTimeElement>, HTMLTimeElement> {
   value: string;
   format?: string | ((value: Moment) => string);
}

const Time = ({ value, format = "HH:mm DD/MM/YYYY", ...props }: TimeProps) => {
   const { className, ...rest } = props;
   return (
      <time className={`text-xs font-light text-default-500 ${className}`} {...rest}>
         {typeof format === "function" ? format(moment(new Date(value))) : moment(new Date(value)).format(format)}
      </time>
   );
};

export default Time;
