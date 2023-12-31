import React from "react";
import { Divider, Link } from "@nextui-org/react";
import NextLink from "next/link";

export interface FooterProps {

}

const Footer = ({}: FooterProps) => {
    return (
        <footer
            className={`w-full  text-center mt-20 p-10 text-large border-t border-t-gray-700`}
        >
            <div className={`flex flex-col justify-center w-full mx-auto items-center gap-2`}>
                <h2 className={`text-foreground font-normal text-sm`}>
                    Chatify, Inc. © {new Date().getFullYear()} |  All rights reserved
                </h2>
                <div className={`flex items-center justify-center text-xxs gap-2`}>
                    <Link as={NextLink} underline={`none`} href={`/about`} className={`cursor-pointer text-xs`}>About</Link>
                    <Divider className={`w-[1px] h-4`} orientation={`vertical`} />
                    <Link as={NextLink} underline={`none`} href={`/contact`} className={`cursor-pointer text-xs`}>Contact</Link>
                    <Divider className={`w-[1px] h-4`} orientation={`vertical`} />
                    <Link as={NextLink} underline={`none`} href={`/help`} className={`cursor-pointer text-xs`}>Help</Link>
                </div>
            </div>
        </footer>
    );
};

export default Footer;
