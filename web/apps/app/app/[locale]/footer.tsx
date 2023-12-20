import React from "react";
import { Divider, Link } from "@nextui-org/react";

export interface FooterProps {

}

const Footer = ({}: FooterProps) => {
    return (
        <footer
            className={`w-full  text-center mt-20 p-10 text-large border-t border-t-gray-700`}
        >
            <div className={`flex flex-col justify-center w-full mx-auto items-center gap-1`}>
                <h2 className={`text-foreground font-normal text-medium`}>
                    @ {new Date().getFullYear()} Chatify © |  All rights reserved
                </h2>
                <div className={`flex items-center justify-center gap-2`}>
                    <Link underline={`none`} href={`/about`} className={`cursor-pointer text-sm`}>About</Link>
                    <Divider className={`w-[1px] h-6`} orientation={`vertical`} />
                    <Link underline={`none`} href={`/contact`} className={`cursor-pointer text-sm`}>Contact</Link>
                    <Divider className={`w-[1px] h-6`} orientation={`vertical`} />
                    <Link underline={`none`} href={`/help`} className={`cursor-pointer text-sm`}>Help</Link>
                </div>
            </div>
        </footer>
    );
};

export default Footer;
